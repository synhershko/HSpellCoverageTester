using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

// This code was borrowed from the BzReader project: http://code.google.com/p/bzreader/

public class WikiPage
{
    /// <summary>
    /// The cached formatted content
    /// </summary>
    //private string formattedContent;
    /// <summary>
    /// Whether this topic is being redirected to a different one
    /// </summary>
    //private string redirectToTopic;
    /// <summary>
    /// The title of the topic
    /// </summary>
    public readonly string Title;
    /// <summary>
    /// The list of block beginnings
    /// </summary>
    public readonly long[] Beginnings;
    /// <summary>
    /// The list of block ends
    /// </summary>
    public readonly long[] Ends;
    /// <summary>
    /// The ID of the Wiki topic
    /// </summary>
    public readonly long TopicId;
    /// <summary>
    /// The score of the hit
    /// </summary>
    public readonly float Score;

    /// <summary>
    /// Whether this topic is being redirected to a different one
    /// </summary>
    //public string RedirectToTopic
    //{
    //    get { return redirectToTopic; }
    //}

    /// <summary>
    /// This constructor is used while indexing the dump to pass the information about topic to tokenizing and
    /// indexing ThreadPool thread
    /// </summary>
    /// <param name="id">Topic ID</param>
    /// <param name="title">Topic title</param>
    /// <param name="begin">The list of the beginnings of the blocks the topic belongs to</param>
    /// <param name="end">The list of the ends of the blocks the topic belongs to</param>
    public WikiPage(long id, string title, long[] begin, long[] end)
    {
        this.TopicId = id;
        this.Title = title;
        this.Beginnings = begin;
        this.Ends = end;
    }

    /// <summary>
    /// This constructor is used while retrieving the hit from the dump
    /// </summary>
    /// <param name="ixr">The dump indexer this Wiki topic belongs to</param>
    /// <param name="hit">The Lucene Hit object</param>
    /*public PageInfo(Indexer ixr, Hit hit)
    {
        Indexer = ixr;

        Score = hit.GetScore();

        Document doc = hit.GetDocument();

        TopicId = Convert.ToInt64(doc.GetField("topicid").StringValue());

        Name = doc.GetField("title").StringValue();

        Beginnings = new long[doc.GetFields("beginning").Length];
        Ends = new long[doc.GetFields("end").Length];

        int i = 0;

        foreach (byte[] binVal in doc.GetBinaryValues("beginning"))
        {
            Beginnings[i] = BitConverter.ToInt64(binVal, 0);

            i++;
        }

        i = 0;

        foreach (byte[] binVal in doc.GetBinaryValues("end"))
        {
            Ends[i] = BitConverter.ToInt64(binVal, 0);

            i++;
        }

        Array.Sort(Beginnings);
        Array.Sort(Ends);
    }*/

    /*
    public string GetFormattedContent()
    {
        if (!String.IsNullOrEmpty(formattedContent))
            return formattedContent;

        //formattedContent = Formatter.Format(Title, GetContentSection(), this, Settings.IsRTL, out redirectToTopic);

        return formattedContent;
    }
    */

    public override string ToString()
    {
        return Title;
    }
}