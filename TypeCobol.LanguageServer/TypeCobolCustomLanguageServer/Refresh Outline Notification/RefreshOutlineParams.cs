using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.LanguageServer.VsCodeProtocol;

namespace TypeCobol.LanguageServer.TypeCobolCustomLanguageServerProtocol
{
    public class RefreshOutlineParams
    {
        /// <summary>
        /// The Text Document URI
        /// </summary>
        public string uri;


        /// <summary>
        /// The List of OutlineNode concerned
        /// </summary>
        public List<OutlineNode> outlineNodes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uri">The Document's identifier</param>
        /// <param name="rootOutlineNode">The List of concerned OutlineNode</param>
        public RefreshOutlineParams(string uri, OutlineNode rootOutlineNode)
        {
            this.uri = uri;
            this.outlineNodes = rootOutlineNode.childNodes;
        }
    }
}
