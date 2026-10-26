"""Generate original, deterministic arcade effects. No external recordings."""
from pathlib import Path
import math, random, struct, wave
out=Path(__file__).resolve().parents[1]/'Assets/Resources/Audio';out.mkdir(parents=True,exist_ok=True)
for number,(name,duration,freq,decay) in enumerate([('Pistol',.20,155,24),('SMG',.12,230,40),('Shotgun',.38,85,13),('Sniper',.48,110,16),('RPG',.55,64,8),('TeleportGun',.34,950,12),('Throw',.15,410,28),('Reload',.22,650,25)]):
 rng=random.Random(712+number); samples=[];low=0
 for i in range(int(24000*duration)):
  t=i/24000;noise=rng.uniform(-1,1);low=.78*low+.22*noise
  if name=='TeleportGun':v=math.sin(2*math.pi*(freq*t+2100*t*t))*math.exp(-decay*t)*.6+noise*.08*math.exp(-30*t)
  elif name=='Reload':v=noise*(math.exp(-180*t)+(.65*math.exp(-180*(t-.12)) if t>=.12 else 0))*.65
  elif name=='Throw':v=low*math.sin(math.pi*t/duration)*.8
  else:v=(noise*.40+low*.65+math.sin(2*math.pi*freq*t*math.exp(-2*t))*.4)*math.exp(-decay*t)
  v*=min(1,t/.002)*min(1,(duration-t)/.015);samples.append(v)
 peak=max(abs(x) for x in samples);samples=[x*.82/max(peak,.001) for x in samples]
 with wave.open(str(out/(name+'.wav')),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(24000);f.writeframes(b''.join(struct.pack('<h',round(x*32767)) for x in samples))
 print(name, len(samples), 'samples; peak',round(max(abs(x) for x in samples),2))
