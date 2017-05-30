using System;
using System.Linq;

namespace ProfileSample.DAL
{
    public class ImagesRepository : IRepository
    {
            private readonly ProfileSampleEntities _context;

            public ImagesRepository()
            {
                _context = new ProfileSampleEntities();
            }

            public IQueryable<ImgSource> GetImages(int count)
            {
                return _context.ImgSources.Take(20);
            }

            public ImgSource GetImageById(int id)
            {
                return _context.ImgSources.Find(id);
            }

            private bool _disposed = false;

            public virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _context.Dispose();
                    }
                }
                _disposed = true;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
    }
    }