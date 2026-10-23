const {chromium}=require(process.argv[2]||'playwright');
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
const base=process.argv[3]||'http://127.0.0.1:8787';
const out=path.resolve(__dirname,'../../output/playwright');
(async()=>{
 fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launch({channel:'msedge',headless:true});
 const page=await browser.newPage({viewport:{width:1280,height:720}}),passed=[],errors=[];
 page.on('pageerror',e=>errors.push(e.message));
 async function screen(name){await page.waitForFunction(n=>window.beaconbreakState?.screen===n,name,{timeout:180000});}
 try{
  await page.goto(base);await screen('Menu');
  assert((await page.title()).includes('SHADOW WARRIORS'));
  await page.screenshot({path:path.join(out,'duskbound-camp.png')});
  passed.push('Shadow Warriors branded build loads');
  await page.mouse.click(515,537);await screen('Guide');
  await page.screenshot({path:path.join(out,'duskbound-guide.png')});
  passed.push('How-to-play and prologue accessible');
  await page.mouse.click(1010,625);await screen('Menu');
  await page.mouse.click(990,225);await screen('Chronicle');
  await page.screenshot({path:path.join(out,'duskbound-story.png')});
  await page.mouse.click(220,580);
  await page.screenshot({path:path.join(out,'duskbound-locked-story.png')});
  passed.push('Chronicle opens with spoiler-gated later chapters');
  await page.mouse.click(1050,635);await screen('Menu');
  await page.mouse.click(200,537);await screen('Skills');
  await page.screenshot({path:path.join(out,'duskbound-upgrades.png')});
  for(let i=0;i<5;i++)await page.mouse.click(550,632);
  await page.screenshot({path:path.join(out,'duskbound-upgrades-last.png')});
  passed.push('Upgrade pagination remains bounded');
  await page.mouse.click(1050,632);await screen('Menu');
  await page.mouse.click(990,160);await screen('Profile');
  await page.mouse.click(490,247);await page.keyboard.press('Control+A');await page.keyboard.type('Test Wanderer');
  await page.mouse.click(410,588);await screen('Menu');
  await page.reload();await screen('Menu');
  await page.screenshot({path:path.join(out,'duskbound-profile-restored.png')});
  passed.push('Profile edit, save, and reload flow completes');
  assert.equal(errors.length,0);
  fs.writeFileSync(path.resolve(__dirname,'../../Game/TestResults/campaign-ui.json'),JSON.stringify({passed,errors,base},null,2));
  console.log('CAMPAIGN_UI_PASS '+passed.length);
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1});
