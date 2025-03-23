namespace MapReader
{
    internal class Program
    {
        static void Main()
        {
            // First map is 12.51*
            // Second map is 12.92*
            // Third map is 10.76*

            for (int i = 0; i < 3; i++)
                MapReader.MapReader.MapReaderCalculator(i, true);
        }
    }
}
