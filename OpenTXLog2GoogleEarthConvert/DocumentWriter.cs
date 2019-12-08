using System;
using System.IO;

namespace OpenTXLog2GoogleEarthConvert
{
    public class DocumentWriter : IDisposable
    {
        private readonly StreamWriter _streamWriter;
        private readonly string _name;
        private readonly bool _visibility;
        private readonly bool _open;
        private readonly string _snippet;
        private bool? _isClosed;

        public DocumentWriter(StreamWriter streamWriter, string name, string snippet) : this(streamWriter, name, snippet, true)
        {

        }
        public DocumentWriter(StreamWriter streamWriter, string name, string snippet, bool open) : this(streamWriter, name, snippet, true, open)
        {

        }
        public DocumentWriter(StreamWriter streamWriter, string name, string snippet, bool visibility, bool open)
        {
            _streamWriter = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
            _name = name;
            _visibility = visibility;
            _open = open;
            _snippet = snippet;
        }

        public void Open()
        {
            _streamWriter.Write($"<Document>");
            _streamWriter.Write($"<name>{_name}</name>");
            _streamWriter.Write($"<visibility>{(_visibility ? 1 : 0)}</visibility>");
            _streamWriter.Write($"<open>{(_open ? 1 : 0)}</open>");
            _streamWriter.Write($"<Snippet>{_snippet}</Snippet>");
            _streamWriter.Write($"<Style id=\"multiTrack\">");
            _streamWriter.Write($"<IconStyle>");
            _streamWriter.Write($"<Icon>");
            _streamWriter.Write($"<href>track0.png</href>");
            _streamWriter.Write($"</Icon>");
            _streamWriter.Write($"</IconStyle>");
            _streamWriter.Write($"<LineStyle>");
            _streamWriter.Write($"<color>00FFFFFF</color>");
            _streamWriter.Write($"<width>6</width>");
            _streamWriter.Write($"</LineStyle>");
            _streamWriter.Write($"</Style>");
            _streamWriter.Write($"<Style id=\"speed_legend\">");
            _streamWriter.Write($"<IconStyle>");
            _streamWriter.Write($"<Icon>");
            _streamWriter.Write($"<href>speed.png</href>");
            _streamWriter.Write($"</Icon>");
            _streamWriter.Write($"</IconStyle>");
            _streamWriter.Write($"</Style>");
            _isClosed = false;
        }
        public void Close()
        {
            _streamWriter.WriteLine($"</Document>");
            _isClosed = true;
        }

        public void Dispose()
        {
            if (_isClosed != null && !_isClosed.Value)
                Close();
        }
    }
}
