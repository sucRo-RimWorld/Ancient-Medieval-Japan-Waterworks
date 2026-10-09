"""Validate the accepted 16 RGBA overlays and all compatible edge pairs."""
from pathlib import Path
import hashlib,json,struct,zlib,re
from PIL import Image
root=Path(__file__).resolve().parents[1]
source=root/'Art/Sources/Terrain/AMJW/Canal'
accepted=json.loads((source/'accepted-export.json').read_text())
tiles={}
for entry in accepted['files']:
    p=root/'Textures/Terrain/AMJW/Canal'/Path(entry['file']).name
    data=p.read_bytes()
    assert hashlib.sha256(data).hexdigest()==entry['sha256'],p
    assert data[:8]==b'\x89PNG\r\n\x1a\n'
    pos=8; packed=b''; done=False
    while pos<len(data):
        n=struct.unpack('>I',data[pos:pos+4])[0]; k=data[pos+4:pos+8]; b=data[pos+8:pos+8+n]
        assert zlib.crc32(k+b)&0xffffffff==struct.unpack('>I',data[pos+8+n:pos+12+n])[0]
        if k==b'IDAT':packed+=b
        pos+=n+12
        if k==b'IEND':done=True;break
    assert done and pos==len(data)
    raw=zlib.decompress(packed);assert len(raw)==128*513
    with Image.open(p) as im:
        assert im.size==(128,128) and im.mode=='RGBA'
        im.load();tiles[entry['mask']]=im.copy()
assert len(tiles)==16
checks=0
for a in range(16):
    for b in range(16):
        if bool(a&2)==bool(b&8):
            assert tiles[a].crop((127,0,128,128)).tobytes()==tiles[b].crop((0,0,1,128)).tobytes();checks+=1
        if bool(a&4)==bool(b&1):
            assert tiles[a].crop((0,127,128,128)).tobytes()==tiles[b].crop((0,0,128,1)).tobytes();checks+=1
assert not list((root/'Textures').rglob('*Wet*'))
geometry=(root/'Source/CanalBedGeometry.cs').read_text()
arrays=re.findall(r'new int\[\] \{ ([\d, ]+) \}',geometry)
assert len(arrays)==32
assert max(len(a.split(',')) for a in arrays)*17*17 < 65535, 'Section mesh exceeds vertex limit'
def coverage(text):
    values=[int(v.strip()) for v in text.split(',')]
    cells=set()
    for i in range(0,len(values),4):
        x0,x1,y0,y1=values[i:i+4]
        assert 0<=x0<x1<=128 and 0<=y0<y1<=128
        rect={(x,y) for y in range(y0,y1) for x in range(x0,x1)}
        assert not cells.intersection(rect), 'Overlapping mesh rectangles'
        cells.update(rect)
    return cells
for m in range(16):
    bed,bank=coverage(arrays[m]),coverage(arrays[m+16])
    assert bed and bank and not bed.intersection(bank), 'Dry shadow overlaps water'
    assert len(bed|bank)==128*128, 'Uncovered pixels at bed/bank boundary'
print(f'[OK] 16 accepted PNG hashes, PNG integrity and {checks} edge pairs; no wet PNGs')
print('[OK] 16 complementary bank/bed meshes: zero shadow coverage over water')
