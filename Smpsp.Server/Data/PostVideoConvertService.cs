namespace Smpsp.Server.Data
{
    public class PostVideoConvertService
    {
        private Thread? _worker;

        public void Start()
        {
            _worker ??= new(Worker) { IsBackground = true };
        }

        private void Worker()
        {

        }
    }
}
