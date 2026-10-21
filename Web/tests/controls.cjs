const {chromium}=require(process.argv[2]);
const fs=require('fs'),path=require('path');
const base=process.argv[3]||'http://127.0.0.1:8787';
const output=path.resolve(__dirname,'../../output/playwright');
fs.mkdirSync(output,{recursive:true});
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true,args:['--disable-background-timer-throttling','--disable-renderer-backgrounding']});
 const desktop=await browser.newContext({viewport:{width:1366,height:768}});
 const phone=await browser.newContext({viewport:{width:844,height:390},hasTouch:true,isMobile:true,deviceScaleFactor:3});
 const host=await desktop.newPage(),guest=await phone.newPage();
 const passed=[];
 // Unity draws at logical 1280x720; account for letterboxing on wide phones.
 async function point(page,x,y){
  const rect=await page.locator('canvas').boundingBox();
  return {x:rect.x+x*rect.width/1280,y:rect.y+y*rect.height/720};
 }
 async function tap(page,x,y){
  const p=await point(page,x,y);await page.touchscreen.tap(p.x,p.y);
 }
 try {
  await host.goto(base); await guest.goto(base);
  for(const page of [host,guest]) await page.waitForFunction(()=>window.beaconbreakState?.screen==='Menu',{},{timeout:180000});
  await host.screenshot({path:path.join(output,'manual-menu.png')});
  const create=await point(host,220,402);
  await host.mouse.click(create.x,create.y); // CREATE ROOM in the observed Unity canvas layout.
  await host.waitForFunction(()=>window.beaconbreakState?.room?.length===8);
  const room=await host.evaluate(()=>window.beaconbreakState.room);
  await guest.getByRole('button',{name:'Join by code',exact:true}).click();
  await guest.getByRole('textbox',{name:'Room code'}).fill(room);
  await guest.getByRole('button',{name:'Join room',exact:true}).click();
  await guest.waitForFunction(()=>window.beaconbreakState?.verified===true);
  await guest.screenshot({path:path.join(output,'manual-lobby-touch.png')});
  await tap(guest,440,472); // I'M READY in logical canvas coordinates.
  await host.waitForFunction(()=>window.beaconbreakState?.screen==='Fight');
  passed.push('Create room and join by visible HTML input; guest readiness');
  const initial=await host.evaluate(()=>window.beaconbreakState.p1X);
  await host.keyboard.down('d'); await new Promise(r=>setTimeout(r,450)); await host.keyboard.up('d');
  await host.waitForFunction(x=>window.beaconbreakState.p1X>x,initial,{timeout:3000});
  passed.push('Keyboard movement changes player 1 position');
  // A physical tap lasts across a render frame; zero-duration synthetic
  // down/up events can both occur before Unity polls keyboard state.
  await host.keyboard.press('k',{delay:100});
  await host.waitForFunction(()=>window.beaconbreakState.p1Attacks>0,{},{timeout:3000});
  passed.push('Keyboard attack reaches simulation');
  const before=await guest.evaluate(()=>window.beaconbreakState.p2X);
  const cdp=await phone.newCDPSession(guest);
  await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{...await point(guest,167,640),id:1}]});
  await new Promise(r=>setTimeout(r,450));
  await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
  await guest.waitForFunction(x=>window.beaconbreakState.p2X>x,before,{timeout:3000});
  passed.push('Touch movement changes player 2 position');
  await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{...await point(guest,987,640),id:2}]});
  await new Promise(r=>setTimeout(r,150));
  await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
  await guest.waitForFunction(()=>window.beaconbreakState.p2Attacks>0,{},{timeout:3000});
  passed.push('Touch attack reaches simulation');
  await host.waitForFunction(()=>window.beaconbreakState.p2Attacks>0,{},{timeout:3000});
  passed.push('Remote touch command reaches authoritative host');
  await guest.screenshot({path:path.join(output,'manual-touch-controls.png')});
  await tap(guest,1200,197); // LEAVE, observed game button.
  await host.waitForFunction(()=>window.beaconbreakState.screen==='Menu',{},{timeout:15000});
  passed.push('Leave button returns partner to menu');
  fs.writeFileSync(path.resolve(__dirname,'../../Game/TestResults/control-tests.json'),JSON.stringify({passed,base,hardware:'Chromium desktop and touch emulation; not physical phones'},null,2));
  console.log('CONTROLS_PASS '+JSON.stringify(passed));
 }catch(e){await host.screenshot({path:path.join(output,'controls-failure-host.png')});await guest.screenshot({path:path.join(output,'controls-failure-guest.png')});throw e}
 finally{await browser.close()}
})().catch(e=>{console.error(e);process.exitCode=1});
