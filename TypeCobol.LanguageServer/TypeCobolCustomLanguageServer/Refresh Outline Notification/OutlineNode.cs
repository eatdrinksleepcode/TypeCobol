using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeModel;
using TypeCobol.Compiler.Nodes;
using TypeCobol.Compiler.Parser;
using TypeCobol.Compiler.Scanner;

namespace TypeCobol.LanguageServer.TypeCobolCustomLanguageServerProtocol
{
    public class OutlineNode
    {
        /// <summary>
        /// The ID of the node
        /// </summary>
        public string id;

        /// <summary>
        /// The node's name
        /// </summary>
        public string name;

        /// <summary>
        /// The node's type
        /// </summary>
        public string type;

        /// <summary>
        /// The OutlineNode parent ID
        /// </summary>
        public string parentId;

        /// <summary>
        /// The node's parent name
        /// </summary>
        public string parentName;

        /// <summary>
        /// The line where the node begins
        /// </summary>
        public int line;

        public bool isUpdated = false;

        /// <summary>
        /// All of the nodes that have this node as parent
        /// </summary>
        public List<OutlineNode> childNodes;

        public OutlineNode(Node node, OutlineNode parent = null)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = node.Name;
            this.type = node.GetType().Name;
            this.parentId = parent?.id;
            this.parentName = node.Parent?.Name;

            var tokensLine = node.Lines.OfType<TokensLine>().FirstOrDefault(l => l.ScanState.InsideFormalizedComment == false && l.ScanState.InsideMultilineComments == false && l.IndicatorChar != '*');

            if (tokensLine != null)
            {
                this.line = tokensLine.LineIndex + 1;
            }
            else if (node is Sentence)
            {
                ReplaceBy(new OutlineNode(node.Children.First(c => c.CodeElement != null), parent));
                return;
            }
            else
            {
                this.line = node.CodeElement?.Line ?? 0;
            }

            var childOutlineNodes = new List<OutlineNode>();

            foreach (var child in node.Children)
            {
                if (child is End == false && child is FunctionEnd == false)
                    childOutlineNodes.Add(new OutlineNode(child, this));
            }

            this.childNodes = childOutlineNodes;
        }

        public bool Update(Node node)
        {
            int i = 0;
            int childrenCount = Math.Max(this.childNodes.Count, node.ChildrenCount);
            this.isUpdated = false;
            while (i < childrenCount)
            {
                if (i >= node.ChildrenCount)
                {
                    if (i >= this.childNodes.Count)
                        break;

                    this.childNodes.RemoveAt(i);
                    this.isUpdated = true;
                    continue;
                }

                if (i >= this.childNodes.Count)
                {
                    if (node.Children[i] is End == false && node.Children[i] is FunctionEnd == false)
                    {
                        this.childNodes.Insert(i, new OutlineNode(node.Children[i], this));
                        this.childNodes[i].isUpdated = true;
                        continue;
                    }
                    break;
                }

                var derivationNode = this.childNodes[i].DerivativeFrom(node.Children[i]);

                if (derivationNode != null)
                {
                    if (derivationNode.Parent is Sentence == false)
                        this.childNodes[i].isUpdated = this.childNodes[i].Update(node.Children[i]);

                    var tokensLine = derivationNode.Lines.OfType<TokensLine>().FirstOrDefault(l => l.ScanState.InsideFormalizedComment == false && l.ScanState.InsideMultilineComments == false && l.IndicatorChar != '*');
                    if (tokensLine != null && this.childNodes[i].line != tokensLine.LineIndex)
                    {
                        this.childNodes[i].line = tokensLine.LineIndex + 1;
                        this.childNodes[i].isUpdated = true;
                    }
                    else if (node.Children[i].CodeElement != null && this.childNodes[i].line != derivationNode.CodeElement.Line || 
                        node.Children[i].CodeElement == null && this.childNodes[i].line != 0)
                    {
                        this.childNodes[i].line = derivationNode.CodeElement?.Line ?? 0;
                        this.childNodes[i].isUpdated = true;
                    }


                }
                else
                {
                    this.childNodes.Insert(i, new OutlineNode(node.Children[i], this));
                    this.childNodes[i].isUpdated = true;
                    childrenCount++;
                }

                i++;
            }

            return this.isUpdated || this.childNodes.Any(c => c.isUpdated);
        }

        public Node DerivativeFrom(Node node)
        {
            if (this.name == node.Name && 
                this.type == node.GetType().Name &&
                this.parentName == node.Parent.Name)
            {
                return node;
            }

            foreach (var child in node.Children)
            {
                Node derivativeNode = DerivativeFrom(child);

                if (derivativeNode != null)
                {
                    return derivativeNode;
                }
            }

            return null;
        }

        public void ReplaceBy(OutlineNode node)
        {
            this.id = node.id;
            this.name = node.name;
            this.type = node.type;
            this.childNodes = node.childNodes;
            this.line = node.line;
            this.parentId = node.parentId;
            this.parentName = node.parentName;
        }
    }
}
