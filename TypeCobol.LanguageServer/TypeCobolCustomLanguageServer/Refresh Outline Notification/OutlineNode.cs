using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeModel;
using TypeCobol.Compiler.Nodes;
using TypeCobol.Compiler.Parser;

namespace TypeCobol.LanguageServer.TypeCobolCustomLanguageServerProtocol
{
    class OutlineNode
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
        public string line;

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

            if (node is Sentence)
            {
                this.line = node.Children.First(c => c.CodeElement != null).CodeElement.Line.ToString();
            }
            else
            {
                this.line = node.CodeElement?.Line.ToString() ?? "";
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
                    }
                }
                else if (this.childNodes[i].DerivativeFrom(node.Children[i]))
                {
                    this.childNodes[i].isUpdated = this.childNodes[i].Update(node.Children[i]);

                    if (node.Children[i] is Sentence && this.childNodes[i].line != node.Children[i].Children.First(c => c.CodeElement != null).CodeElement.Line.ToString())
                    {
                        this.childNodes[i].line = node.Children[i].Children.First(c => c.CodeElement != null).CodeElement.Line.ToString();
                        this.childNodes[i].isUpdated = true;
                    }
                    else if (node.Children[i].CodeElement == null && this.childNodes[i].line == "" || node.Children[i].CodeElement != null && this.childNodes[i].line != node.Children[i].CodeElement.Line.ToString())
                    {
                        this.childNodes[i].line = node.Children[i].CodeElement?.Line.ToString() ?? "";
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

        public bool DerivativeFrom(Node node)
        {
            if (this.name == node.Name && 
                this.type == node.GetType().Name &&
                this.parentName == node.Parent.Name)
            {
                return true;
            }
            return false;
        }
    }
}
