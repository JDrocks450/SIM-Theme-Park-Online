using System;

namespace QuazarAPI.Networking.Standard
{
    public class QEventArgs<T> : EventArgs
    {
        public QEventArgs()
        {

        }
        public QEventArgs(T Data)
        {
            this.Data = Data;
        }
        public T Data { get; set; }
    }
}
