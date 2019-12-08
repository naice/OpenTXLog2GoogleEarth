using System;
using System.IO;

namespace OpenTXLog2GoogleEarthConvert
{
    public class FolderWriter : IDisposable
    {
        private readonly StreamWriter _streamWriter;
        private readonly string _name;
        private readonly bool _visibility;
        private readonly bool _open;

        private bool? _isClosed;

        public FolderWriter(StreamWriter streamWriter, string name) : this(streamWriter, name, true)
        {

        }
        public FolderWriter(StreamWriter streamWriter, string name, bool open) : this(streamWriter, name, true, open)
        {

        }
        public FolderWriter(StreamWriter streamWriter, string name, bool visibility, bool open)
        {
            _streamWriter = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
            _name = name;
            _visibility = visibility;
            _open = open;
        }

        public void Open()
        {
            _streamWriter.WriteLine($"<Folder><name>{_name}</name><visibility>{(_visibility ? 1 : 0)}</visibility><open>{(_open ? 1 : 0)}</open>");
            _isClosed = false;
        }
        public void Close()
        {
            _streamWriter.WriteLine($"</Folder>");
            _isClosed = true;
        }

        public void Dispose()
        {
            if (_isClosed != null && !_isClosed.Value)
                Close();
        }
    }
}
