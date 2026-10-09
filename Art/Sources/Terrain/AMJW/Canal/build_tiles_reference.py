from pathlib import Path
import hashlib, json, shutil, struct, zlib
import numpy as np
from PIL import Image, ImageFilter, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parent
SRC=ROOT.parent/'Waterworks-Isolated-Dry'/'GeneratedMaster.png'
SIZE,SS=128,4
H=SIZE*SS
N,E,S,W=1,2,4,8
SHIFT=5.0
OUT=ROOT/'PNG'
OUT.mkdir(exist_ok=True)
(ROOT/'Sources').mkdir(exist_ok=True)
shutil.copyfile(SRC,ROOT/'Sources'/'ReferenceMaster.png')
original_sha=hashlib.sha256(SRC.read_bytes()).hexdigest()

# Use the existing generated earth source only for restrained surface character.
src=Image.open(SRC).convert('L').crop((450,520,800,790)).resize((H,H),Image.Resampling.LANCZOS)
coarse=src.filter(ImageFilter.GaussianBlur(3.0))
smooth=src.filter(ImageFilter.GaussianBlur(18.0))
detail=(np.asarray(coarse,dtype=float)-np.asarray(smooth,dtype=float))/255
q=(np.arange(H)+.5)/SS-64
xx,yy=np.meshgrid(q,q)
# All geometry/material variation fades to constant in a 3px seam collar.
fade=np.minimum(np.clip((64-np.abs(xx)-3)/14,0,1),np.clip((64-np.abs(yy)-3)/14,0,1))
fade=fade*fade*(3-2*fade)
irregular=(.65*np.sin(xx*.22+yy*.12)+.38*np.sin(xx*.43-yy*.30))*fade
material=detail*fade

def box(x,y,hx,hy,r=0):
    a=np.abs(x)-hx+r;b=np.abs(y)-hy+r
    return np.hypot(np.maximum(a,0),np.maximum(b,0))+np.minimum(np.maximum(a,b),0)-r

def distance(mask,x,y):
    d=box(x,y,11.5,11.5,3.5)
    def merge(a,b):
        h=np.maximum(3.0-np.abs(a-b),0)/3.0
        return np.minimum(a,b)-h*h*.75
    if mask&N: d=merge(d,np.maximum(np.abs(x)-11.5,y))
    if mask&E: d=merge(d,np.maximum(np.abs(y)-11.5,-x))
    if mask&S: d=merge(d,np.maximum(np.abs(x)-11.5,-y))
    if mask&W: d=merge(d,np.maximum(np.abs(y)-11.5,x))
    return d

def name(m): return ''.join(c for c,b in [('N',N),('E',E),('S',S),('W',W)] if m&b) or 'Isolated'

def render(m):
    surface=distance(m,xx,yy)
    outer=surface-14+irregular
    bed=distance(m,xx,yy-SHIFT)+irregular*.45
    coverage=np.clip(.5-outer,0,1)
    floor=np.clip(.5-bed,0,1)
    t=np.clip(bed,0,None)/(np.clip(bed,0,None)+np.clip(-outer,0,None)+1e-8)
    gy,gx=np.gradient(surface)
    ny=gy/np.maximum(np.hypot(gx,gy),1e-6)
    # Fixed screen-space lighting: north inner face is taller and darker.
    side=.32-.11*ny
    bank=(side*(.48+.52*(1-t)) + .15*np.exp(-((t-.12)/.13)**2))
    floor_alpha=.31+material*.30
    shade=(floor_alpha*floor+bank*(1-floor)+material*.48)*coverage
    # Fine broken shoulder light, never an opaque replacement ground colour.
    light=np.clip((t-.77)/.18,0,1)*(.055+.04*np.maximum(ny,0))*coverage
    shade=np.clip(shade,0,.72)
    light=np.clip(light,0,.12)
    # Collapse two conventional alpha passes into a single straight-alpha PNG.
    alpha=1-(1-shade)*(1-light)
    rgb=np.divide(light,alpha,out=np.zeros_like(alpha),where=alpha>1e-8)
    a=np.zeros((H,H,4),dtype=np.uint8)
    a[...,:3]=np.round(rgb[...,None]*255).astype(np.uint8)
    a[...,3]=np.round(alpha*255).astype(np.uint8)
    master=Image.fromarray(a,'RGBA')
    tile=master.resize((SIZE,SIZE),Image.Resampling.BOX)
    arr=np.array(tile)
    arr[arr[...,3]==0,:3]=0
    tile=Image.fromarray(arr,'RGBA')
    support=Image.fromarray(np.uint8(floor*coverage*255),'L').resize((SIZE,SIZE),Image.Resampling.BOX)
    return tile,master,np.array(support)>127

def pngcheck(path):
    data=path.read_bytes(); assert data[:8]==b'\x89PNG\r\n\x1a\n'
    pos=8; packed=b''; header=None; done=False
    while pos<len(data):
        count=struct.unpack('>I',data[pos:pos+4])[0]
        kind=data[pos+4:pos+8];body=data[pos+8:pos+8+count]
        assert len(body)==count
        crc=struct.unpack('>I',data[pos+8+count:pos+12+count])[0]
        assert zlib.crc32(kind+body)&0xffffffff==crc
        if kind==b'IHDR':header=struct.unpack('>IIBBBBB',body)
        if kind==b'IDAT':packed+=body
        pos+=12+count
        if kind==b'IEND':done=True;break
    assert done and pos==len(data)
    assert header==(128,128,8,6,0,0,0)
    raw=zlib.decompress(packed);assert len(raw)==128*(128*4+1)
    assert all(raw[y*513] in range(5) for y in range(128))
    Image.open(path).verify()

def connected(a):
    loc=np.argwhere(a); seen=set(); stack=[tuple(loc[0])]
    while stack:
        p=stack.pop()
        if p in seen:continue
        seen.add(p);y,x=p
        for v,u in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
            if 0<=v<SIZE and 0<=u<SIZE and a[v,u] and (v,u) not in seen:stack.append((v,u))
    return len(seen)==len(loc)

tiles={};supports={};manifest=[]
for m in range(16):
    tile,master,support=render(m)
    filename=f'AMJW_Canal_Dry_{m:02d}_{name(m)}.png'
    tile.save(OUT/filename)
    master.save(ROOT/'Sources'/filename)
    tiles[m]=tile;supports[m]=support
    pngcheck(OUT/filename)
    assert connected(support),('split bed',m)
    edges=[support[0,:],support[:,-1],support[-1,:],support[:,0]]
    for edge,bit in zip(edges,[N,E,S,W]):assert bool(edge.any())==bool(m&bit),(m,bit)
    arr=np.array(tile);assert 0<arr[...,3].max()<255
    assert not arr[:4,:4,3].any() and not arr[-4:,-4:,3].any()
    assert np.array_equal(np.array(render(m)[0]),arr),'non-deterministic'
    manifest.append({'mask':m,'connections':name(m),'file':'PNG/'+filename,'sha256':hashlib.sha256((OUT/filename).read_bytes()).hexdigest()})

checks=0
for a in range(16):
    for b in range(16):
        x,y=np.array(tiles[a]),np.array(tiles[b])
        if bool(a&E)==bool(b&W):
            assert np.array_equal(x[:,-1],y[:,0]),('EW seam',a,b);checks+=1
        if bool(a&S)==bool(b&N):
            assert np.array_equal(x[-1],y[0]),('NS seam',a,b);checks+=1
assert len(set(v['sha256'] for v in manifest))==16
assert hashlib.sha256(SRC.read_bytes()).hexdigest()==original_sha
atlas=Image.new('RGBA',(512,512))
for m,tile in tiles.items():atlas.paste(tile,((m%4)*128,(m//4)*128))
atlas.save(ROOT/'AMJW_Canal_Dry_Atlas_512.png')

# Review sheet: illustrative ground only; never claim these are game frames.
try:font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
except OSError:font=ImageFont.load_default()
sheet=Image.new('RGB',(800,880),(37,36,32));draw=ImageDraw.Draw(sheet)
for m,tile in tiles.items():
    x=(m%4)*200;y=(m//4)*220
    base=Image.new('RGBA',(128,128),(125,104,77,255));base.alpha_composite(tile)
    sheet.paste(base.resize((192,192),Image.Resampling.NEAREST).convert('RGB'),(x+4,y+4))
    draw.text((x+10,y+197),f'{m:02d} {name(m)}',fill=(225,222,210),font=font)
sheet.save(ROOT/'Review_All16.png')

# Narrow joined scene: no filled 2x2 waterway blocks.
cells={(x,3) for x in range(1,8)}|{(4,y) for y in range(1,7)}|{(7,y) for y in range(3,7)}|{(x,6) for x in range(7,10)}|{(x,1) for x in range(1,5)}|{(1,2)}
assert not any(all((x+dx,y+dy) in cells for dx,dy in [(0,0),(1,0),(0,1),(1,1)]) for x,y in cells)
scene=Image.new('RGBA',(11*128,8*128),(125,104,77,255))
for x,y in cells:
    m=sum(bit for dx,dy,bit in [(0,-1,N),(1,0,E),(0,1,S),(-1,0,W)] if (x+dx,y+dy) in cells)
    scene.alpha_composite(tiles[m],(x*128,y*128))
scene.convert('RGB').save(ROOT/'Review_Connected.png')
report={'status':'PASS','tile_count':16,'size':[128,128],'format':'RGBA PNG','bitmask':{'N':1,'E':2,'S':4,'W':8},'bed_offset_screen_pixels':[0,SHIFT],'exact_opposite_edge_checks':checks,'connected_beds':16,'unique_images':16,'source_unchanged':True,'runtime_integrated':False,'art_status':'review','preview_background':'illustrative flat colour, not actual game ground','files':manifest}
(ROOT/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(ROOT/'README.txt').write_text('AMJ Waterworks dry canal, 16 cardinal masks.\n128x128 RGBA PNG. N=1 E=2 S=4 W=8; atlas row-major 0..15.\nBed is shifted 5px screen-down, global lighting maintained across orientations.\nRebuilt isolated tile included; previous isolated source is preserved.\nSource-based deterministic grayscale alpha relief: underlying ground remains visible.\nOriginal ImageGen source material retained under Sources; no new ImageGen call.\nAll opposite-edge compatible pairs are byte-exact; see validation.json.\nReview only: no runtime integration or actual game rendering test.\nPreviews use illustrative ground colours; not game screenshots.\nNo 2x2 wide-waterway variants. Wet state is outside this requested dry set.\n',encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='files'}))
