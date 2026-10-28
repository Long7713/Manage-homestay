import fs from 'node:fs/promises';
const out = 'henry-homestay';
await fs.mkdir(`${out}/assets/images`, {recursive:true});
await fs.mkdir(`${out}/assets/css`, {recursive:true});
await fs.mkdir(`${out}/assets/js`, {recursive:true});
const home = await fs.readFile('trang chủ.html','utf8');
const detail = await fs.readFile('chi tiết phòng.html','utf8');
const urls = [...new Set([...home.matchAll(/src="(https:[^"]+)"/g),...detail.matchAll(/src="(https:[^"]+)"/g)].map(m=>m[1]).filter(s=>s.includes('googleusercontent')))];
const map = {};
for (let i=0;i<urls.length;i++) {
  const name = `assets/images/photo-${String(i+1).padStart(2,'0')}.jpg`;
  try { const r = await fetch(urls[i]); if (!r.ok) throw Error(r.status); await fs.writeFile(`${out}/${name}`, Buffer.from(await r.arrayBuffer())); map[urls[i]]=name; console.log(name); } catch(e) { console.error('DOWNLOAD FAILED',i+1,e.message); }
}
await fs.writeFile(`${out}/assets/image-map.json`, JSON.stringify(map,null,2));
for (const [name,url] of [['tailwind.js','https://cdn.tailwindcss.com?plugins=forms,container-queries']]) {
  const r=await fetch(url); if(!r.ok) throw Error(r.status); await fs.writeFile(`${out}/assets/js/${name}`,await r.text());
}
const fonts='https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&family=Playfair+Display:ital,wght@0,400;0,600;0,700;1,400;1,600&family=Plus+Jakarta+Sans:wght@300;400;500;600;700&family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap';
let css=await (await fetch(fonts)).text();
const fontUrls=[...new Set([...css.matchAll(/url\((https:[^)]+)\)/g)].map(m=>m[1]))];
await fs.mkdir(`${out}/assets/fonts`,{recursive:true});
for(let i=0;i<fontUrls.length;i++){const name=`font-${i}.woff2`;const r=await fetch(fontUrls[i]);await fs.writeFile(`${out}/assets/fonts/${name}`,Buffer.from(await r.arrayBuffer()));css=css.replaceAll(fontUrls[i],`../fonts/${name}`);}
await fs.writeFile(`${out}/assets/css/fonts.css`,css);
console.log('Images:',urls.length,'Fonts:',fontUrls.length);
