using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace net

{
    class Program
    {
        public static Dictionary<string, string> Index = new Dictionary<string, string>();
        public static List<string> msgQueue = new List<string>();
        public static int clock = 0;
        public static int pay = 0;
        public static string uri = "http://127.0.0.1:";
        public static string ThisNode = "0";

        public static string response;
        static void Main(string[] args)
        {
            //read config4.txt to dictionary 
            foreach (string line in File.ReadLines(args[0]))
            {
                if (!line.StartsWith("//") && !string.IsNullOrEmpty(line))
                {
                    if (line.IndexOf("//") > -1)
                    {
                        line.Substring(0, line.IndexOf("//"));

                    }
                    line.Trim();
                    string first = line.Split(' ')[0];
                    string second = line.Split(' ')[1];
                    Index.Add(first, second);

                }

            }

            DoWork();
            Console.ReadLine();

        }

        public static void DoWork()
        {
            try
            {
                response = StartAlgorithm();
                ForwardMessage(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }


        }

        //start the algorithm by sending msg to node1
        public static string StartAlgorithm()
        {
            var node1Port = Index["1"];
            var from = ThisNode;
            var to = "1";
            var tok = 1;
            var initMsg = clock + " " + ThisNode + " " + to + " " + tok + " " + pay;
            Console.WriteLine("... {0} {1} > {2} {3} {4} {5}", clock, ThisNode, from, to, tok, pay);
            var client = new WebClient();
            var res = client.OpenRead(uri + node1Port + "/" + initMsg);
            StreamReader reader = new StreamReader(res);
            var response = reader.ReadToEnd();
            client.Dispose();
            return response;

        }


        //parse the response
        /* while the response is Not to node 0, keep looping*/
        public static void ForwardMessage(string response)
        {
            while (response != null)
            {
                string[] msgList;
                if (response.IndexOf(",") >= 0)
                {
                    msgList = response.Split(',');
                }

                else
                {
                    msgList = new string[1];
                    msgList[0] = response;
                }

                for (var i = 0; i < msgList.Length; i++)
                {
                    string[] msgstrings = msgList[i].Split(' ');
                    int time = Int32.Parse(msgstrings[0]);
                    string from = msgstrings[1];
                    string to = msgstrings[2];
                    int tok = Int32.Parse(msgstrings[3]);
                    string pay = msgstrings[4];
                    int delay = 0;

                    //print incomming message
                    Console.WriteLine("... {0} {1} < {2} {3} {4} {5}", time, ThisNode, from, to, tok, pay);

                    //if the msg is a return msg from node 1 to net,end the loop
                    if (from == "1" & to == "0" & tok == 3)
                    {
                        Console.WriteLine("Algorithm ends");
                  
                        return;
                    }

                    //refer to the delay time    
                    if (Index.ContainsKey(from + "-" + to))
                    {

                        delay = Int32.Parse(Index[from + "-" + to]);
                    }
                    else
                    {
                        delay = Int32.Parse(Index["-"]);
                    }
                    //add delay to msg 
                    time += delay;
                    var msgTosend = time + " " + from + " " + to + " " + tok + " " + pay;
                    //add msg to queue
                    msgQueue.Add(msgTosend);
                }

                //sort the msg
                var tc = new TimeComparer();
                msgQueue.Sort(tc.Compare);

                do
                {
                    //change logical time to the smallest timestamp in the list
                    clock = Int32.Parse(msgQueue[0].Split(' ')[0]);
                    for (var i = 0; i < msgQueue.Count; i++)
                    {
                        var time = Int32.Parse(msgQueue[i].Split(' ')[0]);
                        var from = msgQueue[i].Split(' ')[1];
                        var to = msgQueue[i].Split(' ')[2];
                        var tok = Int32.Parse(msgQueue[i].Split(' ')[3]);
                        var pay = Int32.Parse(msgQueue[i].Split(' ')[4]);
                        string toPort = Index[to];
                        /*if the msg has the same timestamp with the current logical time and remove it in the queue
                        send the msg out one by one and wait back for the response.
                        if the response is null(which is the node receive a vis msg),
                        then change the logical time to the timestamp of the first msg
                        Otherwise, the process will stuck there with no msg in and no msg out*/
                        if (time == clock)
                        {

                            var client = new WebClient();
                            var res = client.OpenRead(uri + toPort + "/" + msgQueue[i]);
                            StreamReader reader = new StreamReader(res);
                            string s = reader.ReadToEnd();
                            msgQueue.Remove(msgQueue[i]);
                            i--;
                            response = s;
                            Console.WriteLine("... {0} {1} > {2} {3} {4} {5}", time, ThisNode, from, to, tok, pay);
                        }
                    }

                } while (response == "" & msgQueue.Count > 0);
            }

        }

    }
}

//sort message by comparing logical time
public class TimeComparer : IComparer<string>
{
    public int Compare(string x, string y)
    {
        string[] xs = x.Split(' ');
        string[] ys = y.Split(' ');
        if (Int32.Parse(xs[0]) != Int32.Parse(ys[0]))
        {
            return Int32.Parse(xs[0]).CompareTo(Int32.Parse(ys[0]));
        }
        else
        {
            if (Int32.Parse(xs[3]) != Int32.Parse(ys[3]))
            {
                if ((xs[3] == "2" & ys[3] == "1") || (xs[3] == "2" & ys[3] == "3") || (xs[3] == "3" & ys[3] == "1"))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (xs[1] != ys[1])
                {
                    return Int32.Parse(xs[1]).CompareTo(Int32.Parse(ys[1]));
                }
                else
                {
                    return Int32.Parse(xs[2]).CompareTo(Int32.Parse(ys[2]));
                }

            }

        }
    }
}

