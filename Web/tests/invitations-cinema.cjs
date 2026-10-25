// Real browser integration: explicit invitations and synchronized story votes.
const {chromium}=require(process.argv[2]||'playwright');
const assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const base=process.argv[3]||'http://127.0.0.1:8787';
const output=path.resolve(__dirname,'../../output/playwright');
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true,args:['--disable-background-timer-throttling','--disable-renderer-backgrounding']});
 const h=await browser.newContext({viewport:{width:1280,height:720},permissions:['clipboard-read','clipboard-write']});
 const g=await browser.newContext({viewport:{width:960,height:540},hasTouch:true});
 const host=await h.newPage(),guest=await g.newPage(),passed=[],errors=[];
 for(const p of [host,guest])p.on('pageerror',e=>errors.push(e.message));
 const wait=(p,fn,arg)=>p.waitForFunction(fn,arg,{timeout:180000});
 async function vote(p){const r=await p.locator('canvas').boundingBox();await p.mouse.click(r.x+r.width*1100/1280,r.y+r.height*685/720);}
 try{
  await host.goto(base);await wait(host,()=>window.beaconbreakState?.screen==='Menu');
  await host.evaluate(()=>window.unityInstance.SendMessage('Co-op Fighter Game','ConfigureBrowser','host|BEEFC099|bot'));
  await host.getByRole('button',{name:'Copy invite link',exact:true}).click();
  const invitation=await host.evaluate(()=>navigator.clipboard.readText());
  assert.equal(new URL(invitation).searchParams.get('room'),'BEEFC099');
  passed.push('Host copies an invitation containing its active room');
  await guest.goto(invitation);await guest.getByRole('button',{name:'Decline',exact:true}).click({timeout:180000});
  // The HTML invitation can be declined before the WebGL runtime loads on a
  // cold public request. Wait for its diagnostic state before asserting it.
  await wait(guest,()=>window.beaconbreakState!=null);
  assert.equal(await guest.evaluate(()=>window.beaconbreakState.verified),false);
  assert.equal(await guest.evaluate(()=>window.bbSocket==null),true);
  passed.push('Declining does not connect or start a fight');
  await guest.goto(invitation+'&test=bot');
  await guest.getByRole('button',{name:'Accept invitation',exact:true}).click({timeout:180000});
  await wait(host,()=>window.beaconbreakState?.screen==='Fight');
  passed.push('Accepted invite connects two separate browser contexts');
  for(const shot of [1,2,3]){
   await wait(host,s=>window.beaconbreakState?.cinema&&window.beaconbreakState.cinemaShot===s,shot);
   await host.screenshot({path:path.join(output,'shadow-cinema-'+shot+'.png')});
  }
  passed.push('First victory plays all three animated shots automatically');
  await wait(host,()=>window.beaconbreakState?.encounter===2&&window.beaconbreakState.cinema);
  await wait(guest,()=>window.beaconbreakState?.encounter===2&&window.beaconbreakState.cinema);
  await vote(host);await wait(host,()=>window.beaconbreakState.skipVoted);
  await host.waitForTimeout(1000);
  assert.equal(await host.evaluate(()=>window.beaconbreakState.cinema),true);
  passed.push('One skip vote cannot dismiss the partner story');
  await vote(guest);
  await wait(host,()=>window.beaconbreakState?.encounter===3);
  passed.push('Two skip votes release the transition and next encounter');
  await wait(host,()=>window.beaconbreakState?.completed?.length===3&&!window.beaconbreakState.cinema);
  await wait(guest,()=>window.beaconbreakState?.completed?.length===3&&!window.beaconbreakState.cinema);
  const a=await host.evaluate(()=>window.beaconbreakState),b=await guest.evaluate(()=>window.beaconbreakState);
  assert.deepEqual(a.completed.map(x=>x.hash),b.completed.map(x=>x.hash));
  passed.push('Later victory also plays; three final match hashes agree');
  assert.equal(errors.length,0);
  fs.writeFileSync(path.resolve(__dirname,'../../Game/TestResults/invitations-cinema.json'),JSON.stringify({passed,errors,base},null,2));
  console.log('INVITATION_CINEMA_PASS '+passed.length);
 }catch(e){await host.screenshot({path:path.join(output,'shadow-cinema-failure.png')}).catch(()=>{});throw e;}
 finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1});
