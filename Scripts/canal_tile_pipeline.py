#!/usr/bin/env python3
"""Deterministic Waterworks EW canal candidate builder.

Development-only: fixes topology in code, derives Dry/Wet from one silhouette,
validates it, then exports only to TestResults. It never promotes production art
or calls an image-generation service.
"""
from __future__ import annotations

import argparse, hashlib, json, math, shutil, struct, tempfile, zlib
from pathlib import Path

TILE, SS = 80, 4
HI = TILE * SS
PNG_SIG = b"\x89PNG\r\n\x1a\n"
SUPPORTED = {"EW"}


def _chunk(kind: bytes, payload: bytes) -> bytes:
    crc = zlib.crc32(kind + payload) & 0xFFFFFFFF
    return struct.pack(">I", len(payload)) + kind + payload + struct.pack(">I", crc)


def write_png(path: Path, rgba: bytes) -> None:
    if len(rgba) != TILE * TILE * 4:
        raise ValueError("RGBA size mismatch")
    raw = b"".join(b"\0" + rgba[y*TILE*4:(y+1)*TILE*4] for y in range(TILE))
    data = PNG_SIG
    data += _chunk(b"IHDR", struct.pack(">IIBBBBB", TILE, TILE, 8, 6, 0, 0, 0))
    data += _chunk(b"IDAT", zlib.compress(raw, 9))
    data += _chunk(b"IEND", b"")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)


def png_integrity(path: Path) -> bool:
    data = path.read_bytes()
    if not data.startswith(PNG_SIG):
        return False
    pos, compressed, ihdr, iend = 8, bytearray(), None, False
    while pos + 12 <= len(data):
        n = struct.unpack(">I", data[pos:pos+4])[0]
        kind, payload = data[pos+4:pos+8], data[pos+8:pos+8+n]
        if pos + 12 + n > len(data):
            return False
        crc = struct.unpack(">I", data[pos+8+n:pos+12+n])[0]
        if zlib.crc32(kind + payload) & 0xFFFFFFFF != crc:
            return False
        pos += 12 + n
        if kind == b"IHDR": ihdr = payload
        elif kind == b"IDAT": compressed.extend(payload)
        elif kind == b"IEND": iend = True; break
    if not ihdr or not iend:
        return False
    w, h, depth, ctype, comp, filt, interlace = struct.unpack(">IIBBBBB", ihdr)
    if (w, h, depth, ctype, comp, filt, interlace) != (80, 80, 8, 6, 0, 0, 0):
        return False
    raw = zlib.decompress(bytes(compressed))
    return len(raw) == TILE * (1 + TILE * 4) and all(raw[y*(1+TILE*4)] == 0 for y in range(TILE))


def _geom(x: float) -> tuple[float, float, float]:
    # Keep one output pixel on both connection edges canonical after downsample.
    if x < SS or x >= HI - SS:
        t, fade = (0.0 if x < SS else 1.0), 0.0
    else:
        t, fade = x / (HI - 1), math.sin(math.pi * x / (HI - 1))
    center = (TILE * .5 - .5) * SS + SS * fade * (.95*math.sin(2*math.pi*t) + .35*math.sin(5*math.pi*t))
    outer = SS * (16 + fade * (1.15*math.sin(3*math.pi*t) + .55*math.sin(7*math.pi*t)))
    bed = SS * (7 + fade * (.45*math.sin(4*math.pi*t) + .25*math.sin(9*math.pi*t)))
    return center, max(SS*13.5, outer), max(SS*5.5, bed)


def _cov(distance: float, radius: float) -> float:
    feather = SS * .85
    return max(0.0, min(1.0, (radius + feather - distance) / feather))


def _render_hi() -> dict[str, bytearray]:
    out = {k: bytearray(HI*HI*4) for k in ("shadow", "highlight", "bed", "water")}
    for y in range(HI):
        for x in range(HI):
            center, outer, bed = _geom(float(x)); d = abs(y-center)
            oc, bc = _cov(d, outer), _cov(d, bed)
            if oc <= 0: continue
            i = (y*HI+x)*4; norm = min(1.0, d/max(1.0, outer))
            out["shadow"][i:i+4] = bytes((0,0,0,int(oc*(38+(1-norm)*62))))
            if y < center and d > bed*.92:
                a = int(oc * max(0.0,min(1.0,(d-bed*.92)/max(1.0,outer-bed))) * 46)
                out["highlight"][i:i+4] = bytes((255,255,255,a))
            if bc > 0:
                out["bed"][i:i+4] = bytes((0,0,0,int(bc*58)))
                if x < SS or x >= HI-SS: variation = 0.0
                else:
                    fade = math.sin(math.pi*x/(HI-1))
                    variation = fade*(5*math.sin(x/(SS*7)) + 3*math.sin((x+y)/(SS*9)))
                rgb = (int(62+variation), int(114+variation), int(132+variation))
                out["water"][i:i+4] = bytes((*rgb,int(bc*178)))
    return out


def _down(src: bytes) -> bytes:
    out = bytearray(TILE*TILE*4); n = SS*SS
    for y in range(TILE):
        for x in range(TILE):
            sums = [0,0,0,0]
            for sy in range(SS):
                for sx in range(SS):
                    i = (((y*SS+sy)*HI)+(x*SS+sx))*4
                    for c in range(4): sums[c] += src[i+c]
            j=(y*TILE+x)*4; out[j:j+4]=bytes(round(v/n) for v in sums)
    return bytes(out)


def _over(a: tuple[int,int,int,int], b: tuple[int,int,int,int]) -> tuple[int,int,int,int]:
    ba, aa = b[3]/255, a[3]/255; oa = ba + aa*(1-ba)
    if oa <= 0: return (0,0,0,0)
    rgb = [round((b[c]*ba + a[c]*aa*(1-ba))/oa) for c in range(3)]
    return (*rgb, round(oa*255))


def _compose(*layers: bytes) -> bytes:
    out=bytearray(TILE*TILE*4)
    for p in range(TILE*TILE):
        i=p*4; pixel=(0,0,0,0)
        for layer in layers: pixel=_over(pixel, tuple(layer[i:i+4]))
        out[i:i+4]=bytes(pixel)
    return bytes(out)


def build_buffers() -> dict[str, bytes]:
    layers={k:_down(v) for k,v in _render_hi().items()}
    bed, water = bytearray(layers["bed"]), bytearray(layers["water"])
    for i in range(3,len(water),4):
        if bed[i] == 0: water[i-3:i+1] = b"\0\0\0\0"
    layers["water"] = bytes(water)
    layers["dry"] = _compose(layers["shadow"], layers["highlight"], layers["bed"])
    layers["wet"] = _compose(layers["shadow"], layers["highlight"], layers["bed"], layers["water"])
    return layers


def validate_buffers(b: dict[str,bytes]) -> dict[str,bool]:
    checks={}
    for name in ("shadow","highlight","bed","water"):
        d=b[name]
        left=b"".join(d[(y*TILE)*4:(y*TILE)*4+4] for y in range(TILE))
        right=b"".join(d[((y*TILE)+79)*4:((y*TILE)+79)*4+4] for y in range(TILE))
        checks[f"{name}_ew_edge_match"] = left == right
    checks["water_inside_bed"] = all(not (b["water"][i] and not b["bed"][i]) for i in range(3,len(b["water"]),4))
    checks["transparent_exterior"] = all(b["dry"][(y*TILE+x)*4+3] == 0 for x,y in ((0,0),(79,0),(0,79),(79,79)))
    open_px=sum(1 for y in range(TILE) if b["water"][(y*TILE)*4+3] > 0)
    checks["ew_open_water_exit"] = 8 <= open_px <= 20
    return checks


def build_and_validate(mask: str, out_dir: Path) -> dict[str,object]:
    mask=mask.upper()
    if mask not in SUPPORTED:
        raise SystemExit(f"Mask {mask!r} is blocked. Only EW baseline generation is enabled until the EW visual target is approved.")
    buffers=build_buffers(); checks=validate_buffers(buffers)
    if not all(checks.values()): raise SystemExit("Mechanical validation failed; candidate was not exported.")
    names={"shadow":"AMJW_Canal_EW_Shadow.png","highlight":"AMJW_Canal_EW_Highlight.png","bed":"AMJW_Canal_EW_Bed.png","water":"AMJW_Canal_EW_Water.png","dry":"AMJW_Canal_Straight_EW_Dry.png","wet":"AMJW_Canal_Straight_EW_Wet.png"}
    parent=out_dir.parent if out_dir.parent.exists() else Path.cwd()
    with tempfile.TemporaryDirectory(prefix="amjw-canal-",dir=parent) as td:
        stage=Path(td)
        for k,n in names.items(): write_png(stage/n,buffers[k])
        checks["png_integrity"] = all(png_integrity(stage/n) for n in names.values())
        if not checks["png_integrity"]: raise SystemExit("PNG integrity failed; candidate was not exported.")
        if out_dir.exists(): shutil.rmtree(out_dir)
        out_dir.mkdir(parents=True)
        for n in names.values(): shutil.copy2(stage/n,out_dir/n)
    hashes={k:hashlib.sha256((out_dir/n).read_bytes()).hexdigest() for k,n in names.items()}
    report={"status":"PASS","mask":"EW","tileSize":80,"candidateOnly":True,"productionApproved":False,"checks":checks,"sha256":hashes}
    (out_dir/"validation.json").write_text(json.dumps(report,indent=2,sort_keys=True)+"\n",encoding="utf-8")
    return report


def main() -> int:
    p=argparse.ArgumentParser(); p.add_argument("--mask",default="EW"); p.add_argument("--out",type=Path,default=Path("TestResults/CanalTiles/EW")); p.add_argument("--all",action="store_true"); a=p.parse_args()
    if a.all: raise SystemExit("--all is blocked until the EW baseline receives explicit visual approval.")
    print(json.dumps(build_and_validate(a.mask,a.out),indent=2,sort_keys=True)); return 0

if __name__ == "__main__": raise SystemExit(main())
