namespace MapToolkit.Contours
{
    internal class ContourSegment
    {
        public ContourSegment(CoordinatesS point1, CoordinatesS point2, double level, int kind, ContourHypothesis? hypothesis = null, int hcase = 0)
        {
            Point1 = point1;
            Point2 = point2;
            Level = level;

            Hypothesis = hypothesis;
            HypothesisCase = hcase;
            Kind = kind;
        }

        public CoordinatesS Point1 { get; }
        public CoordinatesS Point2 { get; }
        public double Level { get; }

        public ContourHypothesis? Hypothesis { get; }
        public int HypothesisCase { get; }
        public int Kind { get; }

        public bool IsValidHypothesis => Hypothesis == null || (Hypothesis.Valid == 0 || Hypothesis.Valid == HypothesisCase);

        public bool IsHypothesis => Hypothesis != null && Hypothesis.Valid == 0;

        public void ValidateHypothesis()
        {
            if (Hypothesis != null)
            {
                Hypothesis.Valid = HypothesisCase;
            }
        }
    }
}
