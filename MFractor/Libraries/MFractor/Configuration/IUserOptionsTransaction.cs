using System;

namespace MFractor.Configuration
{
    public interface IUserOptionsTransaction : IDisposable
    {
        void Commit();

        void Cancel();
    }
}
