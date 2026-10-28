import {spawn} from 'node:child_process';
import fs from 'node:fs/promises';
const inspector='C:/Users/Acer/.codex/plugins/cache/taste-skill/taste-skill/2.1.5/skills/taste-skill/scripts/inspect.mjs';
const pages=[['rooms','rooms'],['login','login'],['register','register'],['food','food'],['booking-detail','booking'],['profile','profile']];
async function run([file,folder]){return new Promise(resolve=>{const p=spawn(process.execPath,[inspector,`henry-homestay/${file}.html`,'--app','--out',`.taste-skill/${folder}`],{windowsHide:true});let output='';p.stdout.on('data',d=>output+=d);p.stderr.on('data',d=>output+=d);p.on('close',async code=>{await fs.writeFile(`.taste-skill/${folder}/inspection.txt`,output);console.log(file,output.match(/RESULT:.*$/m)?.[0]||output.slice(-500));resolve(code);});});}
let failures=0;for(let i=0;i<pages.length;i+=2){const results=await Promise.all(pages.slice(i,i+2).map(run));failures+=results.filter(code=>code!==0).length;}
process.exitCode=failures?1:0;
