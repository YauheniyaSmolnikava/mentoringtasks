using System;
using System.Linq;

namespace ProfileSample.DAL
{
    public interface IRepository : IDisposable
    {
        IQueryable<ImgSource> GetImages(int count);
        ImgSource GetImageById(int id);
    }
}