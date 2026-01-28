namespace Game.Logic
{
    public class Timer
    {
        private int whiteTimeSeconds;
        private int blackTimeSeconds;
        private bool isRunning;
        private Thread timerThread;
        private char currentSide;
        private DateTime lastUpdate;

        public Timer(int timePerSideInSeconds = 600)
        {
            whiteTimeSeconds = timePerSideInSeconds;
            blackTimeSeconds = timePerSideInSeconds;
            isRunning = false;
        }
        
        public void Start(char side)
        {
            if (isRunning)
            {
                Stop();
            }

            currentSide = side;
            isRunning = true;
            lastUpdate = DateTime.Now;

            timerThread = new Thread(TimerCountdown);
            timerThread.IsBackground = true;
            timerThread.Start();
        }
        
        public void Stop()
        {
            isRunning = false;
            if (timerThread != null && timerThread.IsAlive)
            {
                timerThread.Join(100); // Wait briefly for thread to finish
            }
        }
        
        public void SwitchSides()
        {
            char newSide = currentSide == 'w' ? 'b' : 'w';
            Start(newSide);
        }
        
        private void TimerCountdown()
        {
            while (isRunning)
            {
                Thread.Sleep(100); // Update every 100ms for smoother display
                
                DateTime now = DateTime.Now;
                double elapsedSeconds = (now - lastUpdate).TotalSeconds;
                
                if (elapsedSeconds >= 1.0)
                {
                    lastUpdate = now;
                    
                    if (currentSide == 'w')
                    {
                        whiteTimeSeconds--;
                        if (whiteTimeSeconds <= 0)
                        {
                            whiteTimeSeconds = 0;
                            isRunning = false;
                            OnTimeExpired?.Invoke('w');
                        }
                    }
                    else
                    {
                        blackTimeSeconds--;
                        if (blackTimeSeconds <= 0)
                        {
                            blackTimeSeconds = 0;
                            isRunning = false;
                            OnTimeExpired?.Invoke('b');
                        }
                    }
                }
            }
        }
        
        public event Action<char> OnTimeExpired;
        
        public int GetRemainingTime(char side)
        {
            return side == 'w' ? whiteTimeSeconds : blackTimeSeconds;
        }
        
        public string GetFormattedTime(char side)
        {
            int totalSeconds = GetRemainingTime(side);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
        
        public void DisplayTimers()
        {
            Console.WriteLine($"White: {GetFormattedTime('w')} | Black: {GetFormattedTime('b')}");
        }
        
        public bool HasTimeExpired(out char loser)
        {
            if (whiteTimeSeconds <= 0)
            {
                loser = 'w';
                return true;
            }
            if (blackTimeSeconds <= 0)
            {
                loser = 'b';
                return true;
            }
            loser = ' ';
            return false;
        }
        
        public void Reset(int timePerSideInSeconds = 600)
        {
            Stop();
            whiteTimeSeconds = timePerSideInSeconds;
            blackTimeSeconds = timePerSideInSeconds;
        }
        
        public void AddTime(char side, int seconds)
        {
            if (side == 'w')
                whiteTimeSeconds += seconds;
            else
                blackTimeSeconds += seconds;
        }
    }
}