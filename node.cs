using System;
using System.Collections.Generic;
using Nancy.Hosting.Self;
using System.IO;
using Nancy;
using System.Linq;

namespace net
{
    class Program
    {
        public static Dictionary<string, string> NodePort = new Dictionary<string, string>();
        public static bool VisitedStatus = false;
        public static Dictionary<string, string> Edges = new Dictionary<string, string>();
        public static Dictionary<string, string> Unvisited = new Dictionary<string, string>();
        public static int clock = 0;
        public static int payload = 1;
        public static string ThisNode;

        static void Main(string[] args)
        {

            // string line;
            //read config4.txt(args[0]) to dictionary 

            foreach (string line in File.ReadLines(args[0]))
            {

                if (line.IndexOf("-") == -1 && !line.StartsWith("//") && !string.IsNullOrEmpty(line))
                {

                    if (line.IndexOf("//") > -1)
                    {
                        line.Substring(0, line.IndexOf("//"));
                    }
                    line.Trim();
                    string first = line.Split(' ')[0];
                    string second = line.Split(' ')[1];
                    NodePort.Add(first, second);

                }


            }


            //read nodeNum from argument
            ThisNode = args[1];

            //add neighbours to the Edges dictionary 
            Edges = new Dictionary<string, string>();
            for (var i = 2; i < args.Length; i++)
            {

                Edges.Add(args[i], "unvisited");

            }

            //add unvisited edge to dictionary
            if (Edges.ContainsValue("unvisited"))
            {

                foreach (var edge in Edges)
                {
                    if (edge.Value == "unvisited")
                    {

                        Unvisited.Add(edge.Key, edge.Value);

                    }
                }
            }




            var configuration = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = false }
            };

           configuration.RewriteLocalhost = false;

            using (var host = new NancyHost(configuration, new Uri("http://127.0.0.1:" + NodePort[ThisNode])))

            {

                host.Start();
                Console.ReadLine();

            }

        }

    }
    public class SendMessage : NancyModule
    {
        public static string parent;
        public static string child;
        public string Msg { get; set; }
        public static string uri = "http://127.0.0.1:";

        enum Tokens
        {
            forward = 1,
            vis = 2,
            returning = 3
        }
        public SendMessage()
        {
            Get("/{msg}", parameters =>
            {
                /* start the algorithm*/
                string message = parameters.msg;
                string[] msgstring = message.Split(' ');
                int time = Int32.Parse(msgstring[0]);
                string from = msgstring[1];
                string to = msgstring[2];
                int tok = Int32.Parse(msgstring[3]);
                int pay = Int32.Parse(msgstring[4]);
                //print incomming message
                Console.WriteLine("... {0} {1} < {2} {3} {4} {5}", time, Program.ThisNode, from, to, tok, pay);

                List<string> msgTobeSent = new List<string>();
                Program.clock = time;

                /*when receive the foward token: the node will 
                    1. if unvisited:
                      -if has unvisited neigh:
                         mark itself visited, send fwd send vis tok
                     - if has no unvisited neigh
                         return tok, send vis 
                    2. if visited: mark neigh visited
              */
                if (tok == (int)Tokens.forward)
                {
                    if (Program.VisitedStatus == false)
                    {
                        Program.VisitedStatus = true;

                        //mark the parent
                        if (Program.Edges.ContainsKey(from))
                        {
                            parent = "from";
                            Program.Edges[from] = "parent";
                            Program.Unvisited.Remove(from);
                        }
                        //if there is still unvisted edge: select the unvisited edges to a new list
                        if (Program.Unvisited.Count > 0)
                        {
                            var forwardMsg = SelectUnvistedEdge(Program.Unvisited, Program.clock, pay);
                            msgTobeSent.Add(forwardMsg);
                        }
                        //if there is no unvisited edge, return the token
                        else
                        {

                            var returnMsg = Program.clock + " " + Program.ThisNode + " " + from + " " + (int)Tokens.returning + " " + Program.payload;
                            msgTobeSent.Add(returnMsg);
                        }

                        //send visited msg to visited & unvisted edges, not including parent and child edges    
                        foreach (var edge in Program.Edges)
                        {
                            if (edge.Value != "parent" & edge.Value != "child")
                            {
                                var visitedMsg = Program.clock + " " + Program.ThisNode + " " + edge.Key + " " + (int)Tokens.vis + " " + pay;//!!!***update pay here
                                msgTobeSent.Add(visitedMsg);
                            }

                        }
                    }
                    else
                    {
                        Program.Edges[from] = "visited";
                        Program.Unvisited.Remove(from);
                    }
                }

                /* receiving vis tok
                   1.if from child, mark it visited:-if still unvisited neigh, send fwd tok, else return
                 */
                else if (tok == (int)Tokens.vis)
                {
                    Program.Unvisited.Remove(from);

                    if (Program.Edges[from] == "child")
                    {
                        if (Program.Unvisited.Count > 0)
                        {
                            var forwardMsg = SelectUnvistedEdge(Program.Unvisited, Program.clock, pay);
                            msgTobeSent.Add(forwardMsg);
                        }
                        else
                        {
                            //if no record parent in the list, it's node 1 and its parent is 0
                            var parent = "0";
                            foreach (var edge in Program.Edges)
                            {
                                if (edge.Value == "parent")
                                {
                                    parent = edge.Key;
                                }
                            }
                            var returnMsg = Program.clock + " " + Program.ThisNode + " " + parent + " " + (int)Tokens.returning + " " + Program.payload;
                            msgTobeSent.Add(returnMsg);
                        }
                    }
                    Program.Edges[from] = "visited";

                }
                //receive return token: if still unvisited neigh, send fwd tok, else return to parent
                else
                {
                    Program.payload += pay;
                    //if there remains unvisited eddges
                    if (Program.Unvisited.Count > 0)
                    {
                        var forwardMsg = SelectUnvistedEdge(Program.Unvisited, Program.clock, 0);
                        msgTobeSent.Add(forwardMsg);
                    }
                    //if all the edges of the node are visited 
                    else
                    {
                        var parent = "0";
                        if (to != "1")
                        {
                            foreach (var edge in Program.Edges)
                            {
                                if (edge.Value == "parent")
                                {
                                    parent = edge.Key;
                                }
                            }
                        }

                        var returnMsg = Program.clock + " " + Program.ThisNode + " " + parent + " " + (int)Tokens.returning + " " + Program.payload;
                        msgTobeSent.Add(returnMsg);

                    }
                }
                /* end the algorithm*/

                //send msg to net               
                for (var i = 0; i < msgTobeSent.Count; i++)
                {
                    Console.WriteLine("... {0} {1} > {2}", time, Program.ThisNode, msgTobeSent[i].Substring(msgTobeSent[i].IndexOf(" ") + 1));//!!!***remove time
                }

                return String.Join(",", msgTobeSent);
            });
        }


        //select the next unvisted edges to probe        
        //remove child from unvisited and mark child in Edges
        private string SelectUnvistedEdge(Dictionary<string, string> Unvisited, int clock, int pay)
        {
            //Random random = new Random();
            //  var index = random.Next(Program.Unvisited.Count);
            child = Unvisited.ElementAt(0).Key;
            /*  foreach (var u in Unvisited)
              {
                  if (u.Value == "univisited")
                  {
                      child = u.Key;
                      Console.WriteLine(child);
                      break;
                  }


              }*/
            var forwardMsg = Program.clock + " " + Program.ThisNode + " " + child + " " + (int)Tokens.forward + " " + pay;
            Program.Edges[child] = "child";
            Program.Unvisited.Remove(child);

            return forwardMsg;
        }

    }
}