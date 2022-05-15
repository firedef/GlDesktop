using System.Diagnostics;

namespace GlDesktop.gl.render.renderer; 

public class RendererTimer {
	public int frequency = 60;
	public bool useThreadSleepForRenderDelay = true;
	
	public float renderTimeDelta { get; private set; }
	private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

	public RendererTimer(int frequency) {
		this.frequency = frequency;
	}

	public void Wait() {
		long msPerUpdate = 1000 / frequency;
		
		while (true) {
			long delay = msPerUpdate - _stopwatch.ElapsedMilliseconds;
			if (delay <= 0) {
				renderTimeDelta = (float) _stopwatch.Elapsed.TotalMilliseconds;
				_stopwatch.Restart();
				return;
			}
			
			if (!useThreadSleepForRenderDelay) continue;
			
			int sleepMs = (int)(msPerUpdate - delay);
			if (sleepMs > 1) Thread.Sleep(sleepMs);
		}
	}
}