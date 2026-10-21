const {chromium}=require(process.argv[2]);
const fs=require('fs'),path=require('path');
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try {
  const page=await browser.newPage({viewport:{width:1280,height:720}});
  page.on('pageerror',e=>console.log('PAGE_ERROR '+e.message));
  page.on('console',m=>console.log(m.type()+': '+m.text().slice(0,500)));
  await page.goto('http://127.0.0.1:8787');
  const output=path.resolve(__dirname,'../../output/playwright');fs.mkdirSync(output,{recursive:true});
  for(let i=0;i<30;i++) {
   if(await page.evaluate(()=>!!window.unityInstance))break;
   if(i%3===0) {console.log(await page.evaluate(()=>({progress:document.querySelector('progress')?.value,message:document.querySelector('#message')?.textContent})));await page.screenshot({path:path.join(output,'loading-debug.png')});}
   await new Promise(r=>setTimeout(r,2000));
  }
  if(!await page.evaluate(()=>!!window.unityInstance))throw Error('WebGL startup timed out');
  await new Promise(r=>setTimeout(r,5000));
  await page.screenshot({path:path.join(output,'menu-smoke.png')});
  console.log('MENU_SMOKE_PASS');
 } finally {await browser.close()}
})().catch(e=>{console.error(e);process.exitCode=1});
