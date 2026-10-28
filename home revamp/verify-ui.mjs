import fs from 'node:fs/promises';
import path from 'node:path';
import {pathToFileURL} from 'node:url';
import {Browser} from 'file:///C:/Users/Acer/.codex/plugins/cache/taste-skill/taste-skill/2.1.5/skills/taste-skill/scripts/lib/cdp.mjs';
const root=path.resolve('henry-homestay');
const browser=await Browser.launch();
const report={viewports:[],interactions:[],errors:[]};
function assert(condition,message){if(!condition)throw Error(message);report.interactions.push(message);}
try{
  const p=await browser.newPage({width:1440,height:900});
  for(const name of ['index','room-detail','rooms','login','register','food','booking-detail','profile']){
    for(const width of [1440,1366,768,390]){
      await p.setViewport({width,height:900});
      await p.goto(pathToFileURL(path.join(root,name+'.html')).href,{settleMs:100});
      await p.eval(async()=>{await document.fonts.ready;await Promise.all([...document.images].map(async i=>{i.loading='eager';try{await i.decode();}catch{}}));});
      const result=await p.eval(()=>({overflow:document.documentElement.scrollWidth>innerWidth+1,brokenImages:[...document.images].filter(i=>!i.complete||!i.naturalWidth).map(i=>i.src),title:document.title,h1:document.querySelector('h1')?.innerText,external:[...document.querySelectorAll('img[src],script[src],link[href]')].map(e=>e.src||e.href).filter(s=>/^https?:/.test(s))}));
      report.viewports.push({name,width,...result});
      if(result.overflow)report.errors.push(`${name}@${width}: horizontal overflow`);
      if(result.brokenImages.length)report.errors.push(`${name}@${width}: broken images`);
      if(result.external.length)report.errors.push(`${name}@${width}: remote assets`);
    }
  }
  const go=async name=>p.goto(pathToFileURL(path.join(root,name+'.html')).href,{settleMs:100});
  await go('rooms');
  assert(await p.eval(()=>document.querySelectorAll('.room-card').length===6),'Six sample rooms render');
  await p.eval(()=>{document.querySelector('[value=studio]').checked=true;document.getElementById('room-filters').dispatchEvent(new Event('input',{bubbles:true}));});
  assert(await p.eval(()=>document.querySelectorAll('.room-card').length===1),'Type filter updates list and count');
  await p.eval(()=>{document.getElementById('max-price').value='1000000';document.getElementById('room-filters').dispatchEvent(new Event('input',{bubbles:true}));});
  assert(await p.eval(()=>!!document.getElementById('empty-reset')),'Empty state offers reset');
  await p.eval(()=>document.getElementById('empty-reset').click());
  await p.eval(()=>{const s=document.getElementById('sort');s.value='price-desc';s.dispatchEvent(new Event('change'));});
  assert(await p.eval(()=>document.querySelector('.room-card h3').textContent==='Family Suite 81'),'Price sort is descending');
  await p.eval(()=>{localStorage.removeItem('henry-saved');document.querySelector('.favorite').click();});
  assert(await p.eval(()=>Henry.readSaved().includes('81')),'Favourite persists in localStorage');
  await go('profile');await p.eval(()=>{location.hash='saved';});
  assert(await p.eval(()=>document.querySelector('#saved-rooms .room-card h3')?.textContent==='Family Suite 81'),'Saved rooms appear in profile');
  await p.eval(()=>document.querySelector('[data-remove]').click());
  assert(await p.eval(()=>!!document.querySelector('#saved-rooms .empty')),'Removing final saved room shows empty state');
  await p.eval(()=>{location.hash='personal';});
  await p.eval(()=>{const f=document.getElementById('profile-form');f.elements.name.value='Khách xem mẫu';f.requestSubmit();});
  assert(await p.eval(()=>document.getElementById('summary-name').textContent==='Khách xem mẫu'),'Profile updates display without backend');
  await go('register');
  await p.eval(()=>{const f=document.getElementById('register-form');f.elements.name.value='Khách mẫu';f.elements.email.value='khach@henry.example';f.elements.password.value='demo-password';f.elements.confirm.value='different';f.elements.terms.checked=true;f.requestSubmit();});
  assert(await p.eval(()=>!document.getElementById('confirm-password').validity.valid),'Registration rejects mismatched passwords');
  await p.eval(()=>{const f=document.getElementById('register-form');f.elements.confirm.value='demo-password';f.elements.confirm.dispatchEvent(new Event('input'));f.requestSubmit();});
  assert(await p.eval(()=>!document.getElementById('auth-message').hidden),'Registration confirms visual prototype only');
  await go('login');
  await p.eval(()=>document.querySelector('[data-password]').click());
  assert(await p.eval(()=>document.getElementById('login-password').type==='text'),'Password visibility toggle works');
  await p.eval(()=>{const f=document.getElementById('login-form');f.elements.email.value='khach@henry.example';f.elements.password.value='demo-password';f.requestSubmit();});
  assert(await p.eval(()=>!document.getElementById('auth-message').hidden&&document.getElementById('login-password').value===''),'Login returns demo feedback and clears password');
  await p.eval(()=>document.querySelector('[data-open-dialog]').click());
  assert(await p.eval(()=>document.getElementById('forgot-dialog').open),'Forgot-password dialog opens');
  await go('food');
  await p.eval(()=>document.querySelector('[data-category=drink]').click());
  assert(await p.eval(()=>[...document.querySelectorAll('.food-article')].filter(a=>!a.hidden).length===2),'Food category filters the two drinks');
  await p.eval(()=>document.querySelector('[data-article=ca-phe]').click());
  assert(await p.eval(()=>document.getElementById('article-dialog').open&&document.getElementById('article-title').textContent.includes('cà phê')),'Food article opens readable content');
  await go('booking-detail');
  assert(await p.eval(()=>document.querySelector('[data-cost=total]').textContent.includes('2.430.000')&&document.querySelector('[data-cost=remaining]').textContent==='0 ₫'),'Booking arithmetic reconciles with zero outstanding');
  await go('room-detail');
  await p.eval(()=>document.querySelector('[onclick="openBookingModal()"]').click());
  await new Promise(r=>setTimeout(r,200));
  assert(await p.eval(()=>location.pathname.endsWith('booking-detail.html')),'Retained room booking action opens demo booking detail');
  await go('rooms');
  await p.setViewport({width:390,height:844});
  await p.eval(()=>document.querySelector('.mobile-toggle').click());
  assert(await p.eval(()=>document.querySelector('.main-nav').classList.contains('is-open')),'Mobile navigation opens');
  if(p.consoleErrors.length)report.errors.push(...p.consoleErrors);
  await p.close();
}catch(e){report.errors.push(e.stack);}finally{await browser.close();}
await fs.writeFile('.taste-skill/ui-verification.json',JSON.stringify(report,null,2));
console.log(JSON.stringify({viewportChecks:report.viewports.length,interactions:report.interactions.length,errors:report.errors},null,2));
if(report.errors.length)process.exitCode=1;
