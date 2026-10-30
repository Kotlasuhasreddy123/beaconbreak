// Story-route integration using isolated saves and real host/guest sockets.
const {chromium}=require(process.argv[2]||'playwright');
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const base=process.argv[3]||'http://127.0.0.1:8787';
const level=Number(process.argv[4]||10);
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true,args:['--disable-background-timer-throttling','--disable-renderer-backgrounding']});
 const contexts=await Promise.all([browser.newContext({viewport:{width:1280,height:720}}),browser.newContext({viewport:{width:844,height:390},hasTouch:true,isMobile:true})]);
 const pages=await Promise.all(contexts.map(c=>c.newPage())),errors=[],shots=[];
 const room='D'+Math.random().toString(16).slice(2,9).toUpperCase();
 const wait=(p,f,a)=>p.waitForFunction(f,a,{timeout:180000});
 try{
  for(const p of pages){p.on('pageerror',e=>errors.push(e.message));await p.goto(base);await wait(p,()=>window.beaconbreakState?.screen==='Menu');}
  for(let i=0;i<2;i++)await pages[i].evaluate(v=>window.unityInstance.SendMessage('Co-op Fighter Game','ConfigureBrowser',v),`${i?'guest':'host'}|${room}|bot|${level}`);
  await wait(pages[0],()=>window.beaconbreakState?.screen==='Fight');
  await wait(pages[1],()=>window.beaconbreakState?.screen==='Fight');
  await pages[1].locator('#join-tools').waitFor({state:'hidden'});
  await pages[1].locator('#fullscreen').waitFor({state:'hidden'});
  assert.equal((await pages[0].evaluate(()=>window.beaconbreakState)).level,level);
  await pages[0].screenshot({path:path.resolve(`output/playwright/guardian-${level}-fight.png`)});
  await pages[1].screenshot({path:path.resolve(`output/playwright/guardian-${level}-phone.png`)});
  for(const shot of [1,2,3]){
   await wait(pages[0],n=>window.beaconbreakState?.cinema&&window.beaconbreakState.cinemaShot===n,shot);
   await pages[0].waitForTimeout(1800);
   const file=path.resolve(`output/playwright/guardian-${level}-shot-${shot}.png`);await pages[0].screenshot({path:file});shots.push(file);
  }
  await wait(pages[1],()=>window.beaconbreakState?.completed?.length>=1);
  const results=await Promise.all(pages.map(p=>p.evaluate(()=>window.beaconbreakState.completed[0])));
  assert.equal(results[0].hash,results[1].hash);assert.equal(results[0].level,level);
  assert.equal(results[0].result,level===100?'credits':'report');
  if(level!==100){await wait(pages[0],()=>window.beaconbreakState?.encounter===2);assert.equal(await pages[0].evaluate(()=>window.beaconbreakState.level),level+10);}
  else {
   await wait(pages[0],()=>window.beaconbreakState?.screen==='Credits'&&!window.beaconbreakState.cinema);
   await pages[0].screenshot({path:path.resolve('output/playwright/guardian-ending-credits.png')});
  }
  assert.deepEqual(errors,[]);
  fs.writeFileSync(path.resolve(`Game/TestResults/guardian-browser-${level}.json`),JSON.stringify({base,level,results,shots,errors},null,2));
  console.log('GUARDIAN_BROWSER_PASS '+level);
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
