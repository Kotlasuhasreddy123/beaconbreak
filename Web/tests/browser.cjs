// Run with: node tests/browser.cjs <path-to-playwright-package> [base-url]
// Two independent browser contexts model two devices. This does not claim
// physical iOS/Android hardware coverage.
const { chromium } = require(process.argv[2] || 'playwright');
const fs = require('fs');
const path = require('path');
const base = process.argv[3] || 'http://127.0.0.1:8787';
const output = path.resolve(__dirname, '../../output/playwright');
fs.mkdirSync(output, {recursive:true});
(async () => {
  const browser = await chromium.launch({channel:'msedge',headless:true,args:['--disable-background-timer-throttling','--disable-renderer-backgrounding','--autoplay-policy=no-user-gesture-required']});
  const hostContext = await browser.newContext({viewport:{width:1280,height:720}});
  const guestContext = await browser.newContext({viewport:{width:960,height:540},hasTouch:true,isMobile:true});
  const host = await hostContext.newPage(), guest = await guestContext.newPage();
  const errors=[];
  for(const [role,page] of [['host',host],['guest',guest]]) {
    page.on('pageerror',e=>errors.push(role+': '+e.message));
    page.on('console',m=>{if(m.type()==='error') console.log(role+' console: '+m.text().slice(0,300));});
  }
  try {
    await host.goto(base);
    await host.waitForFunction(()=>window.unityInstance,{},{timeout:180000});
    await new Promise(r=>setTimeout(r,5000));
    await host.screenshot({path:path.join(output,'menu-desktop.png')});
    console.log('PASS: WebGL loaded and menu rendered');
    await host.evaluate(()=>window.unityInstance.SendMessage('Co-op Fighter Game','ConfigureBrowser','host|BEEFC012|bot'));
    await guest.goto(base+'/?role=guest&room=BEEFC012&test=bot');
    await guest.getByRole('button',{name:'Accept invitation',exact:true}).click({timeout:180000});
    await guest.waitForFunction(()=>window.beaconbreakState?.screen==='Fight',{},{timeout:180000});
    await host.waitForFunction(()=>window.beaconbreakState?.tick>200,{},{timeout:60000});
    await host.screenshot({path:path.join(output,'fight-desktop.png')});
    await guest.screenshot({path:path.join(output,'fight-mobile-emulation.png')});
    console.log('PASS: independent desktop and touch contexts joined the room');
    for (let i=0; i<90; i++) {
      const a=await host.evaluate(()=>window.beaconbreakState);
      const b=await guest.evaluate(()=>window.beaconbreakState);
      if(i%5===0)console.log(JSON.stringify({host:a,guest:b}));
      if(a?.completed?.length===3 && b?.completed?.length===3)break;
      await new Promise(r=>setTimeout(r,2000));
    }
    const a=await host.evaluate(()=>window.beaconbreakState);
    const b=await guest.evaluate(()=>window.beaconbreakState);
    if(a.completed.length!==3||b.completed.length!==3)throw Error('Three encounters did not complete');
    for(let i=0;i<3;i++) {
      if(a.completed[i].hash!==b.completed[i].hash || a.completed[i].tick!==b.completed[i].tick)throw Error('Final state mismatch in encounter '+i);
    }
    if(a.completed[0].level!==a.completed[1].level)throw Error('Same-level retry not exercised');
    if(errors.length)throw Error(errors.join('\n'));
    await host.screenshot({path:path.join(output,'transfer-report.png')});
    await guest.close();
    await host.waitForFunction(()=>window.bbSocket===null,{},{timeout:15000});
    await host.screenshot({path:path.join(output,'partner-disconnected.png')});
    const report={passed:true,mode:'two isolated Chromium contexts; desktop + touch viewport',host:a,guest:b,errors};
    report.base=base;
    fs.writeFileSync(path.resolve(__dirname,'../../Game/TestResults/'+(base.startsWith('https:')?'browser-tests-public.json':'browser-tests.json')),JSON.stringify(report,null,2));
    console.log('BROWSER_PASS: 3 encounters, same-level restart, next-level transition, identical final states, disconnect');
  } catch(error) {
    await host.screenshot({path:path.join(output,'failure-host.png')}).catch(()=>{});
    await guest.screenshot({path:path.join(output,'failure-guest.png')}).catch(()=>{});
    throw error;
  } finally { await browser.close(); }
})().catch(e=>{console.error(e);process.exitCode=1;});
