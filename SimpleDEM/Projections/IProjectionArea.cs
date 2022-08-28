namespace SimpleDEM.Projections
{
    public interface IProjectionArea
    {
        Vector Project(Coordinates coordinates);

        Vector Min { get; }

        Vector Size { get; }
    }
}