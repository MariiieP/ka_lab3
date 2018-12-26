namespace ka_lb3
{
    internal class Ant
    {
        private int alpha;
        private int beta;
        private double rho;
        private double q;
        private int numAnts;
        private int maxTime;

        public Ant(int alpha, int beta, double rho, double q, int numAnts, int maxTime)
        {
            this.alpha = alpha;
            this.beta = beta;
            this.rho = rho;
            this.q = q;
            this.numAnts = numAnts;
            this.maxTime = maxTime;
        }
    }
}