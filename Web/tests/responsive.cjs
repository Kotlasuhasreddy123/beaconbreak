// Run: node tests/responsive.cjs <playwright-module-path> [server-url]
// Measures real rendered canvas bounds; does not claim physical device coverage.
const {chromium}=require(process.argv[2]||'playwright');
const assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const base=process.argv[3]||'http://127.0.0.1:8787';
const sizes=[[320,568],[360,640],[375,667],[390,844],[412,915],[430,932],
 [568,320],[640,360],[667,375],[844,390],[915,412],[932,430],
 [768,1024],[1024,768],[820,1180],[1180,820],
 [1280,720],[1366,768],[1920,1080],[2560,1080]];
const output=path.resolve(__dirname,'../../output/playwright');
const report={base,scope:'Edge/Chromium screen-size and touch emulation, not physical hardware',cases:[],errors:[]};
(async()=>{
 fs.mkdirSync(output,{recursive:true});
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const context=await browser.newContext({viewport:{width:1280,height:720},hasTouch:true,deviceScaleFactor:2});
  const page=await context.newPage();
  page.on('pageerror',e=>report.errors.push(e.message));
  await page.goto(base);
  await page.waitForFunction(()=>window.beaconbreakState?.screen==='Menu',null,{timeout:180000});
  for(const [width,height] of sizes){
   await page.setViewportSize({width,height});
   // Allow Unity's render-buffer resize and the responsive CSS to settle.
   await page.waitForTimeout(400);
   const measured=await page.evaluate(()=>{
    const r=document.querySelector('canvas').getBoundingClientRect();
    return {x:r.x,y:r.y,width:r.width,height:r.height,
     viewportWidth:innerWidth,viewportHeight:innerHeight,
     overflow:document.documentElement.scrollWidth>innerWidth,
     rotate:getComputedStyle(document.querySelector('#rotate')).display!=='none'};
   });
   assert(measured.width>0&&measured.height>0,'Canvas has positive dimensions');
   assert(Math.abs(measured.width/measured.height-16/9)<0.005,'Preserved arena ratio');
   assert(measured.x>=-1&&measured.y>=-1,'No clipping at top/left');
   assert(measured.x+measured.width<=width+1&&measured.y+measured.height<=height+1,'Canvas fits viewport');
   assert(!measured.overflow,'No horizontal page overflow');
   assert.equal(measured.rotate,height>width,'Portrait orientation hint');
   await page.getByRole('button',{name:'Join by code',exact:true}).click();
   const dialog=await page.locator('dialog').boundingBox();
   assert(dialog.x>=0&&dialog.x+dialog.width<=width+1,'Join dialog fits');
   await page.getByRole('button',{name:'Cancel',exact:true}).click();
   report.cases.push({width,height,passed:true,measured});
   if([320,844,1024,1366,2560].includes(width))
    await page.screenshot({path:path.join(output,'layout-'+width+'x'+height+'.png')});
  }
  assert.equal(report.errors.length,0,'No JavaScript errors');
  console.log('RESPONSIVE_PASS '+report.cases.length+' viewports, including rotation and join dialog');
 }finally{
  fs.writeFileSync(path.resolve(__dirname,'../../Game/TestResults/responsive-tests.json'),JSON.stringify(report,null,2));
  await browser.close();
 }
})().catch(e=>{console.error(e);process.exitCode=1});
