using System;
using System.Linq;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.WebServer.VFS
{
    public abstract class VFSEntry
    {
        private string _Name = "entry";
        public virtual string Name
        {
            get { return _Name; }
            set { _Name = value ?? "entry"; }
        }
    }

    public abstract class VFSFile : VFSEntry
    {
        public abstract void Execute(Context Context);
    }

    public class VFSDirectory : VFSEntry
    {
        private List<VFSEntry> _Entrys = new List<VFSEntry>();
        private bool _EnableDirectoryListing;

        public List<VFSEntry> Entrys { get { return _Entrys; } }
        public VFSEntry this[int Index] { get { return _Entrys[Index]; } }
        public bool EnableDirectoryListing { get { return _EnableDirectoryListing; } set { _EnableDirectoryListing = value; } }

        public VFSDirectory(string Name, bool EnableDirectoryListing = false) 
        {
            this.Name = Name;
            _EnableDirectoryListing = EnableDirectoryListing;
        }

        public void Add(VFSEntry Entry) { _Entrys.Add(Entry); }

        public void AddRange(IEnumerable<VFSEntry> Entrys) { _Entrys.AddRange(Entrys); }

        public void Remove(VFSEntry Entry) { _Entrys.Remove(Entry); }

        public void RemoveAt(int Index) { _Entrys.RemoveAt(Index); }

        public int RemoveAll(Predicate<VFSEntry> Predicate) { return _Entrys.RemoveAll(Predicate); }

        private VFSFile CheckForIndexFile(List<VFSEntry> Entrys)
        {
            foreach (VFSEntry entry in Entrys)
                if (entry is VFSFile)
                {
                    VFSFile entryFile = entry as VFSFile;
                    if (entryFile.Name == "index")
                        return entryFile;
                }
            return null;
        }

        private VFSFile ReturnIndexOrListOrNull(VFSDirectory Directory)
        {
            VFSFile indexFile = CheckForIndexFile(Directory.Entrys);
            if (indexFile != null)
                return indexFile;
            else if (EnableDirectoryListing)
                return new VFSDirectoryListingFile(Directory, true);
            else return null;
        }

        private VFSFile internalGetFile(List<string> pathParts, VFSDirectory currentDirectory)
        {
            if (pathParts.Count < 1)
                return ReturnIndexOrListOrNull(currentDirectory);
            else foreach (VFSEntry entry in currentDirectory.Entrys)
                    if (entry.Name.Equals(pathParts[0]))
                        if (entry is VFSFile)
                            return entry as VFSFile;
                        else if (entry is VFSDirectory)
                        {
                            VFSDirectory entryDir = entry as VFSDirectory;
                            if (pathParts.Count > 1)
                                return internalGetFile(pathParts.GetRange(1, pathParts.Count - 1), entryDir);
                            else return ReturnIndexOrListOrNull(entryDir);
                        }
            return null;
        }

        public VFSFile GetFile(string Path)
        {
            string[] pathParts = Path.Split(new char[2] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            return internalGetFile(pathParts.ToList(), this);
        }
    }
}
