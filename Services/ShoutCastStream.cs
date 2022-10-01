using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RipShout.Services;



public delegate void StreamTitleChangedHandler(object source, string title, string genre, string bitrate, string extension);

public class ShoutCastStream : Stream
{
    private int metaInt = 16000;
    private int receivedBytes;
    private Stream? netStream;
    private bool connected = false;
    private string streamGenre = "NA";
    private string streamTitle = "NA";
    private string bitRate = "NA";
    public string AudioEncodeType = "aac";

    /// <summary>
    /// Is fired when a new StreamTitle is received
    /// </summary>
    public event StreamTitleChangedHandler StreamTitleChanged;

    public async Task<bool> StartUp(string url)
    {
        // Check for playlist url
        if(url.Contains(".pls"))
        {
            //text = Regex.Replace(text,
            //@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
            //    "<a target='_blank' href='$1'>$1</a>");
        }

        HttpClient client = new HttpClient();
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Method = HttpMethod.Get,
        };
        request.Headers.Add("Icy-MetaData", "1");

        await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ContinueWith((tm) =>
        {
            var response = tm.Result;
            var metaIntTemp = response.Headers.First(a => a.Key == "icy-metaint").Value.First();
            int.TryParse(metaIntTemp, out metaInt);
            bitRate = response.Headers.First(a => a.Key == "icy-br").Value.First();
            streamGenre = response.Headers.First(a => a.Key == "icy-genre").Value.First();

            if(response.Headers.Any(a => a.Key == "Content-Type"))
            {
                var conType = response.Headers.FirstOrDefault(a => a.Key == "Content-Type").Value.First();
                if(!string.IsNullOrEmpty(conType) && conType.ToLower().StartsWith("audio"))
                {
                    AudioEncodeType = conType.Replace("audio/", "");
                }
            }
            netStream = response.Content.ReadAsStreamAsync().Result;
        });
        return true;
    }

    public ShoutCastStream()
    {
    }

    private static NetworkCredential ParseCreds(string url)
    {
        Regex regCreds = new Regex(@"http://(.+?):(.+?)@.+");
        Match mCreds = regCreds.Match(url);
        if(mCreds.Success)
        {
            return new NetworkCredential(mCreds.Groups[1].Value, mCreds.Groups[2].Value);
        }
        else
        {
            throw new Exception("Parse NetworkCreds Failed");
        }
    }

    /// <summary>
    /// Parses the received Meta Info
    /// </summary>
    /// <param name="metaInfo"></param>
    private void ParseMetaInfo(byte[] metaInfo)
    {
        string metaString = Encoding.ASCII.GetString(metaInfo);

        string newStreamTitle = Regex.Match(metaString, "(StreamTitle=')(.*)?'").Groups[2].Value.Trim();
        if(!newStreamTitle.Equals(streamTitle))
        {
            streamTitle = newStreamTitle;
            OnStreamTitleChanged();
        }
    }

    /// <summary>
    /// Fires the StreamTitleChanged event
    /// </summary>
    protected virtual void OnStreamTitleChanged()
    {
        if(StreamTitleChanged != null)
        {
            StreamTitleChanged(this, streamTitle, streamGenre, bitRate, AudioEncodeType);
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the ShoutcastStream supports reading.
    /// </summary>
    public override bool CanRead { get { return connected; } }

    /// <summary>
    /// Gets a value that indicates whether the ShoutcastStream supports seeking. This property will always be false.
    /// </summary>
    public override bool CanSeek { get { return false; } }

    /// <summary>
    /// Gets a value that indicates whether the ShoutcastStream supports writing. This property will always be false.
    /// </summary>
    public override bool CanWrite { get { return false; } }

    /// <summary>
    /// Gets the title of the stream
    /// </summary>
    public string StreamTitle { get { return streamTitle; } }

    public string BitRate { get { return bitRate; } }

    public string StreamGenre { get { return streamGenre; } }

    /// <summary>
    /// Flushes data from the stream. This method is currently not supported
    /// </summary>
    public override void Flush()
    {
        return;
    }

    /// <summary>
    /// Gets the length of the data available on the Stream. This property is not currently supported and always thows a
    /// <see cref="NotSupportedException"/>.
    /// </summary>
    public override long Length { get { throw new NotSupportedException(); } }

    /// <summary>
    /// Gets or sets the current position in the stream. This property is not currently supported and always thows a
    /// <see cref="NotSupportedException"/>.
    /// </summary>
    public override long Position
    {
        get { throw new NotSupportedException(); }
        set { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Reads data from the ShoutcastStream.
    /// </summary>
    /// <param name="buffer">An array of bytes to store the received data from the ShoutcastStream.</param>
    /// <param name="offset">The location in the buffer to begin storing the data to.</param>
    /// <param name="count">The number of bytes to read from the ShoutcastStream.</param>
    /// <returns>The number of bytes read from the ShoutcastStream.</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        try
        {
            if(netStream == null)
            {
                connected = false;
                return -1;
            }
            if(receivedBytes == metaInt)//if(receivedBytes == 16000)
            {
                int metaLen = netStream.ReadByte();
                if(metaLen > 0)
                {
                    byte[] metaInfo = new byte[metaLen * 16];
                    int len = 0;
                    while((len += netStream.Read(metaInfo, len, metaInfo.Length - len)) < metaInfo.Length)
                    {
                        ;
                    }
                    ParseMetaInfo(metaInfo);
                }
                receivedBytes = 0;
            }

            int bytesLeft = ((metaInt - receivedBytes) > count) ? count : (metaInt - receivedBytes);
            int result = netStream.Read(buffer, offset, bytesLeft);
            receivedBytes += result;
            return result;
        }
        catch(Exception)
        {
            connected = false;
            return -1;
        }
    }

    /// <summary>
    /// Closes the ShoutcastStream.
    /// </summary>
    public override void Close()
    {
        connected = false;
        if(netStream != null)
        {
            netStream.Close();
        }
    }

    /// <summary>
    /// Sets the current position of the stream to the given value. This Method is not currently supported and always
    /// throws a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Sets the length of the stream. This Method always throws a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="value"></param>
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Writes data to the ShoutcastStream. This method is not currently supported and always throws a <see
    /// cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
