import fs from 'node:fs/promises';
const root='henry-homestay';
const map=JSON.parse(await fs.readFile(`${root}/assets/image-map.json`,'utf8'));
for(const [source,target,key] of [['trang chủ.html','index.html','home'],['chi tiết phòng.html','room-detail.html','detail']]){
  let html=await fs.readFile(source,'utf8');
  html=html.replace(/<html[^>]*>/,'<html lang="vi">');
  html=html.replace(/<title>[\s\S]*?<\/title>/,`<title>${key==='home'?'Trang chủ':'Chi tiết phòng'} | HENRY homestay</title>`);
  if(!html.includes('<title>'))html=html.replace('<head>','<head><title>Chi tiết phòng | HENRY homestay</title>');
  for(const [url,path] of Object.entries(map))html=html.replaceAll(url,path);
  html=html.replace(/<link[^>]*href="https:\/\/fonts\.[^>]*>/g,'');
  html=html.replace('</head>','<link rel="stylesheet" href="assets/css/fonts.css"><link rel="stylesheet" href="assets/css/legacy-fixes.css"></head>');
  html=html.replace(/src="https:\/\/cdn.tailwindcss.com[^"]*"/g,'src="assets/js/tailwind.js"');
  html=html.replaceAll('2TWOV','HENRY').replaceAll('2Twov','HENRY').replaceAll('2twov','HENRY').replaceAll('STAYEASE SANCTUARY','HENRY HOMESTAY').replaceAll('StayEase Sanctuary','HENRY homestay').replaceAll('StayEase','HENRY');
  html=html.replace('transition-transform">2</div>','transition-transform">H</div>');
  html=html.replaceAll('vanviet1683006@gmail.com','hello@henry.example').replaceAll('hoangminh.design@gmail.com','khach@henry.example').replaceAll('0795443500','0000000000').replaceAll('+84 795443500','Chưa cập nhật').replaceAll('0988 123 456','').replaceAll('001201008899','').replaceAll('Nguyễn Hoàng Minh','Khách trải nghiệm').replaceAll('Quốc Huy','Khách mẫu');
  html=html.replaceAll('98888 666888','DEMO — không thanh toán').replaceAll('98888666888','DEMO').replaceAll('MB Bank (Ngân hàng Quân Đội)','Ngân hàng minh họa');
  html=html.replace(/href="tel:[^"]*"/g,'href="mailto:hello@henry.example"');
  html=html.replace(/href="#am-thuc"/g,'href="food.html"');
  html=html.replace('</body>','<script src="assets/js/legacy-navigation.js"></script></body>');
  let n=0;const styles=[];
  html=html.replace(/<style([^>]*)>([\s\S]*?)<\/style>/g,(_,attrs,css)=>{if(attrs.includes('tailwind'))return _;styles.push(css);return '';});
  if(styles.length){await fs.writeFile(`${root}/assets/css/legacy-${key}.css`,styles.join('\n'));html=html.replace('</head>',`<link rel="stylesheet" href="assets/css/legacy-${key}.css"></head>`);}
  const scripts=[];html=html.replace(/<script([^>]*)>([\s\S]*?)<\/script>/g,(all,attrs,js)=>{if(attrs.includes('src=')||!js.trim())return all;const name=`legacy-${key}-${++n}.js`;scripts.push([name,js]);return `<script src="assets/js/${name}"></script>`;});
  for(const [name,js] of scripts)await fs.writeFile(`${root}/assets/js/${name}`,js);
  await fs.writeFile(`${root}/${target}`,html);
}
// Use DESIGN.md's Inter throughout the new pages. Retained pages keep their own type.
for(const name of ['base.css','pages.css']){const path=`${root}/assets/css/${name}`;let css=await fs.readFile(path,'utf8');css=css.replaceAll('font-family:Georgia,serif','font-family:var(--font)');await fs.writeFile(path,css);}
// Keep the handoff legible without requiring a formatter or a build system.
for(const name of ['index.html','room-detail.html','rooms.html','login.html','register.html','food.html','booking-detail.html','profile.html']){const path=`${root}/${name}`;let html=await fs.readFile(path,'utf8');html=html.replace(/>\s*</g,'>\n<');await fs.writeFile(path,html);}
for(const name of ['base.css','pages.css']){const path=`${root}/assets/css/${name}`;let css=await fs.readFile(path,'utf8');css=css.replace(/\{/g,' {\n  ').replace(/;/g,';\n  ').replace(/\}/g,'\n}\n').replace(/\n\s+\n/g,'\n');await fs.writeFile(path,css);}
console.log('Prepared 8 HTML pages; original files unchanged.');
