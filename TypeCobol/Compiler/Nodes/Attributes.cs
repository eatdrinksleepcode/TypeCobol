﻿
using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace TypeCobol.Compiler.Nodes {

    using System.Collections.Generic;
    using System.Linq;
    using Tools;
    using TypeCobol.Compiler.CodeElements;
    using TypeCobol.Compiler.CodeElements.Expressions;
    using TypeCobol.Compiler.CodeModel;



    public static class Attributes
    {
	    internal static object Get(Node node, string attribute) {
		    var table = node.SymbolTable;
		    object value = node;
		    try {
			    foreach(var attr in attribute.Split('.')) {
				    if (value == null) break;
				    value = attributes[attr.ToLower()].GetValue(value, table);
			    }
			    return value;
		    } catch (KeyNotFoundException) {
			    DEFAULT.Key = attribute;
			    return DEFAULT.GetValue(value, table);
		    }
	    }

	    private static Dictionary<string,Attribute> attributes;
	    static Attributes() {
		    attributes = new Dictionary<string,Attribute>();
		    attributes["name"]  = new NameAttribute();
		    attributes["level"] = new LevelAttribute();
            attributes["value"] = new ValueAttribute();
            attributes["type"]  = new TypeAttribute();
		    attributes["sender"] = new SenderAttribute();
	        attributes["receiver"] = new ReceiverAttribute();
            attributes["unsafe"] = new UnsafeAttribute();
		    attributes["function"] = new FunctionUserAttribute();
		    attributes["definitions"] = new DefinitionsAttribute();
	        attributes["usage"] = new UsageAttribute();
	        attributes["hash"] = new HashAttribute();
	        attributes["displayablereceivers"] = new PointerDisplayableReceiversAttribute();
	        attributes["receivers"] = new PointerReceiversAttribute();
            attributes["receiverusage"] = new ReceiverUsageAttribute();
	        attributes["incrementdirection"] = new incrementDirectionAttribute();
	        attributes["needcompute"] = new NeedComputeAttribute();
	        attributes["ispointerincrementation"] = new IsPointerIncrementationAttribute();
	        attributes["isnested"] = new IsNestedAttribute();
	        attributes["containnested"] = new ContainNestedAttribute();
            attributes["global"] = new GlobalAttribute();
            attributes["useglobalstoragevariable"] = new UseGlobalStorageVariableAttribute();
            attributes["sourceprogramhash"] = new SourceProgramHashAttribute();
            //not used?
            attributes["typecobol"] = new TypeCobolAttribute();
		    attributes["visibility"] = new VisibilityAttribute();
		    attributes["copyname"] = new LibraryCopyAttribute();
		    attributes["programname8"] = new ProgramName8Attribute();
            attributes["imports"] = new ProgramImportsAttribute();
	    }
	    private static ContainerAttribute DEFAULT = new ContainerAttribute();
    }

    internal class ContainerAttribute: Attribute
    {
	    internal string Key { get; set; }
	    public object GetValue(object o, SymbolTable table) {
		    return null;
	    }
    }



    public interface Attribute
    {
	    object GetValue(object o, SymbolTable table);
    }

    internal class NameAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = o as Node;
            if (node != null)
            {
                var named = ((Node)o).CodeElement as NamedCodeElement;
                return named?.Name;
            }
            else
                return null;
        }
    }

    internal class UsageAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = o as DataDescription;
            if (node != null)
            {
                var usage = node.Usage;
                return usage;
            }
            else
                return null;
        }
    }

    internal class ReceiverUsageAttribute : Attribute
    {

        public object GetValue(object o, SymbolTable table)
        {
            var node = (Node)o;
            var codeElement = node.CodeElement;
            var setStatement = codeElement as SetStatementForIndexes;
            List<List<DataDefinition>> variablesWrittenRaw = new List<List<DataDefinition>>();
            List<DataDefinition> variablesWritten = new List<DataDefinition>();
            if (setStatement != null && node.IsFlagSet(Node.Flag.NodeContainsPointer))
            {
                foreach (var data in setStatement.StorageAreaWrites)
                {
                    variablesWrittenRaw.Add(
                        node.SymbolTable.GetVariablesExplicitWithQualifiedName(data.MainSymbolReference.URI).Select(x => x.Value).ToList());
                }

                foreach (var varWritten in variablesWrittenRaw)
                {
                    if (varWritten.Count > 1)
                        throw new Exception("Ambiguous Receiver Name");// Ambiguous variable handeling is done before, in the checkers
                    else
                        variablesWritten.Add(varWritten.First());
                }

            }
            if (variablesWritten == null) return null;
            if (variablesWritten.Count == 0) return null;
            else return variablesWritten.First().Usage;
        }
    }
    internal class HashAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = o as DataDescription;
            if (node != null)
            {
                var hash = node.Hash;
                return hash;
            }
            else
                return null;
        }
    }
    internal class TypeAttribute: Attribute
    {
	    public object GetValue(object o, SymbolTable table) {
            bool result;
            if(bool.TryParse(o.ToString(), out result)) {
                return "BOOL";
            }

		    var node = o as DataDescription;
	        if (node != null) {
                    var data = node.CodeElement;
	            if (data != null) {
                        return /*data.Picture!=null? data.Picture.Value :*/ data.UserDefinedDataType != null ? data.UserDefinedDataType.Name : null;
                    }
                }
	        return null;

	    }
    }

    /// <summary>
    /// Class to evaluate the value of a DataDescription node if its associated DataDescriptionEntry
    /// Code Element has an Initial value.
    /// </summary>
    internal class ValueAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var data = o as DataDescription;
            if (data == null) return null;
            TypeCobol.Compiler.CodeElements.Value value = data.CodeElement.InitialValue;
            if (value != null)
            {
                if (value.LiteralType == Value.ValueLiteralType.Boolean)
                {   //Special case for TypeCobol Bool type value.
                    return value.BooleanValue.Value ? "'T'" : "'F'";
                }
                return value.ToString();
            }
            return "";
        }
    }

    internal class LevelAttribute: Attribute
    {
	    public object GetValue(object o, SymbolTable table)
	    {
		    var data = o as DataDefinition;
		    if (data == null) return null;
		    return string.Format("{0:00}", data.CodeElement?.LevelNumber?.Value);
	    }
    }

    internal class GlobalAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var dataDef = o as DataDefinition;
            var data = dataDef?.CodeElement as DataDescriptionEntry;

            if (data == null) return null;
            if (data.Global != null && data.Global.Value)
            {
                return " " + data.Global.Token.Text;
            }
            else
                return "";
        }
    }
    internal class UseGlobalStorageVariableAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            if (o is Program pgm)
            {
                return pgm.IsFlagSet(Node.Flag.UseGlobalStorage);
            }
            else if (o is FunctionDeclaration fun)
            {
                return fun.IsFlagSet(Node.Flag.UseGlobalStorage);
            }
            
            else
                return false;
        }
    }

    /// <summary>
    /// Gives access to the hash of the main program in which a given Node is defined.
    /// Returns <code>null</code> when retrieved on anything that is not a Node.
    /// </summary>
    internal class SourceProgramHashAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            return o is Node node ? node.Root.MainProgram.Hash : null;
        }
    }

    internal class TypeCobolAttribute: Attribute
    {
	    internal string Key { get; set; }
	    public object GetValue(object o, SymbolTable table)
	    {
		    var map = o as IDictionary<StorageArea,object>;
		    var results = new Dictionary<StorageArea,object>();
	        if (map != null)
	            foreach (var kv in map)
	                if (kv.Key.SymbolReference is TypeCobolQualifiedSymbolReference)
	                    results.Add(kv.Key,kv.Value);
	        return results;
	    }
    }

    internal class SenderAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = (Node)o;
            var ce = node.CodeElement;

            // Used for bool arithmetics
            var setCondition = ce as SetStatementForConditions;
            if (setCondition != null)
                return new URI(setCondition.SendingValue.Value.ToString());
            else if (node.IsFlagSet(Node.Flag.NodeContainsPointer))
            {
                // Used for pointers arithmetics
                var setIndex = ce as SetStatementForIndexes;
                if (setIndex?.SendingVariable != null)
                {
                    if (setIndex.SendingVariable.MainSymbolReference != null)
                        // the sender is a qualified name
                        return setIndex.SendingVariable.MainSymbolReference.ToString().Replace(" IN ", " OF ");
                    else if (setIndex.SendingVariable.ArithmeticExpression != null)
                        // the sender is an arithmetic expression
                        return setIndex.SourceText.ToLower().Split(new[] {" by "}, StringSplitOptions.None)[1];
                    return new URI(setIndex.SendingVariable.ToString());
                }
            }
            return null;
        }
    }

    internal class NeedComputeAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = (Node)o;
            if (node.IsFlagSet(Node.Flag.NodeContainsPointer))
            {
                var setIndex = (node as Set)?.CodeElement as SetStatementForIndexes;
                if (setIndex?.SendingVariable?.ArithmeticExpression != null)
                    return true;
            }
            return false;
        }
    }
    internal class IsPointerIncrementationAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = (Node)o;
            return node.IsFlagSet(Node.Flag.NodeisIncrementedPointer);
        }
    }

    internal class IsNestedAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var fun = o as FunctionDeclaration;
            if (fun == null) return null;
            return fun.GenerateAsNested.ToString();
        }
    }

    internal class ContainNestedAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            if (o is Program == false) return false;

            return DefinitionsAttribute.GetFunctionsGeneratedAsNested(o as Program).Public.Count > 0;
        }
    }

    internal class incrementDirectionAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = (Node)o;
            var ce = node.CodeElement;
            var setIndex = ce as SetStatementForIndexes;
            if (setIndex != null)
                return setIndex.IncrementDirection.ToString();
            return null;
        }
    }
    
    internal class ReceiverAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var codeElement = ((Node)o).CodeElement;
            var variablesWritten = codeElement.StorageAreaWrites;
            List<URI> variablesURI = new List<URI>();
            if (variablesWritten == null) return null;
            if (variablesWritten.Count == 0) return null;
            foreach (var codeElementStorageAreaWrite in variablesWritten)
            {
                variablesURI.Add(new URI(codeElementStorageAreaWrite.ToString()));
            }
            return variablesURI;
        }
    }
    internal class PointerDisplayableReceiversAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = (Node)o;
            List<string> displayableWritten = new List<string>();
            if (node.CodeElement is SetStatementForIndexes && node.IsFlagSet(Node.Flag.NodeContainsPointer))
            {
                foreach (var data in node.StorageAreaWritesDataDefinition)
                {
                    // Usage of Regex.Replace to replace only the first ooccurence of dataDef.Name to avoid probleme with groups like myPtrGroup::myPtr
                    var regex = new Regex(Regex.Escape(data.Value.Name));
                    displayableWritten.Add(
                        regex.Replace(
                            data.Key.ToString(), data.Value.Name + data.Value.Hash, 1)
                            .Replace(" IN ", " OF "));
                }

            }
            if (displayableWritten.Count == 0) return null;
            return displayableWritten;
            
        }
    }
    internal class PointerReceiversAttribute : Attribute
    {
        public object GetValue(object o, SymbolTable table)
        {
            var node = (Node)o;
            if (node.CodeElement is SetStatementForIndexes)
            {
                return node.StorageAreaWritesDataDefinition.Values;
            }
            return null;
        }
    }

    internal class UnsafeAttribute: Attribute {
	    public object GetValue(object o, SymbolTable table) {
		    var statement = o as VariableWriter;
		    if (statement == null) return null;
		    return statement.IsUnsafe;
	    }
    }

    internal class FunctionUserAttribute: Attribute {
	    public object GetValue(object o, SymbolTable table) {
		    var statement = o as FunctionCaller;

            string functionName = statement?.FunctionCall?.FunctionName;
            if (functionName == null) return null;

		    var found = table.GetFunction(new URI(functionName));
            switch (found.Count)
            {
                case 0:
                    return null;
                case 1:
                    return Create(statement.FunctionCall, found[0]);
                default:
                    throw new ArgumentException("Resolve ambiguity for " + found.Count + " items");
            }
        }

	    private static FunctionCallInfo Create(FunctionCall call, FunctionDeclaration declaration) {
            System.Diagnostics.Debug.Assert(call.FunctionName != null);
		    var result = new FunctionCallInfo(new URI(call.FunctionName), declaration.Library, declaration.Copy);
		    if (declaration.Profile == null) return result;
		    int count = declaration.Profile.InputParameters.Count + declaration.Profile.InoutParameters.Count + declaration.Profile.OutputParameters.Count;
		    // declaration.Profile.ReturningParameter is not used because
		    // the same data is always used by (and hardcoded in) function call codegen: <function.Name>-RESULT
		    for(int i=0; i < count; i++) {
			    var pAsDefined = GetParameter(i, declaration);
			    var pAsUsed    = GetParameter(i, call);
			    result.InputParameters.Add(Create(pAsDefined, pAsUsed));
		    }
		    return result;
	    }
	    private static CallParameter Create(ParameterDescription pAsDefined, CallParameter pAsUsed) {
		    if (pAsUsed != null) return pAsUsed; //Code is strange here..
		    return new EmptyCallParameter();
	    }
	    private static ParameterDescription GetParameter(int index, FunctionDeclaration function) {
		    int offset = 0;
		    if (index - offset < function.Profile.InputParameters.Count) return function.Profile.InputParameters[index-offset];
		    offset += function.Profile.InputParameters.Count;
		    if (index - offset < function.Profile.InoutParameters.Count) return function.Profile.InoutParameters[index-offset];
		    offset += function.Profile.InoutParameters.Count;
		    if (index - offset < function.Profile.OutputParameters.Count) return function.Profile.OutputParameters[index-offset];
		    offset += function.Profile.OutputParameters.Count;
		    if (index - offset < 1) return function.Profile.ReturningParameter;
		    throw new System.ArgumentOutOfRangeException("Expected: "+index+" < "+function.Profile.InputParameters.Count
		                                                                     +'+'+function.Profile.InoutParameters.Count
		                                                                     +'+'+function.Profile.OutputParameters.Count
		                                                                     +'+'+(function.Profile.ReturningParameter!=null?1:0));
	    }
	    private static CallParameter GetParameter(int index, FunctionCall function) {
		    if (function.Arguments != null && index < function.Arguments.Length) return new CallParameter(function.Arguments[index].StorageAreaOrValue);
		    return null;
	    }
    }

    public class FunctionCallInfo
    {
        /// <summary>Used for codegen.</summary>
        public FunctionCallInfo(QualifiedName name, string lib, string copy)
        {
            QualifiedName = name;
            Library = lib;
            Copy = copy;
        }

        public string Name { get { return QualifiedName.Head; } }
        public QualifiedName QualifiedName { get; private set; }

        public IList<CallParameter> InputParameters = new List<CallParameter>();

        public string Library { get; set; }
        public string Copy { get; set; }
    }

    public class CallParameter
    {

        private Variable voe;
        public CallParameter(Variable voe) { this.voe = voe; }

        public virtual bool IsLiteral { get { return voe.IsLiteral; } }
        public virtual string SendingMode { get { return IsLiteral ? "CONTENT" : "REFERENCE"; } }
        public virtual string Value { get { return voe.MainSymbolReference != null ? voe.MainSymbolReference.Name : null; } }
    }
    public class EmptyCallParameter : CallParameter
    {
        public EmptyCallParameter() : base(null) { }
        public override bool IsLiteral { get { return true; } }
        public override string SendingMode { get { return "CONTENT"; } }
        public override string Value { get { return "SPACE"; } }
    }

internal class DefinitionsAttribute: Attribute {
	public object GetValue(object o, SymbolTable table) {
		var definitions = new Definitions();
		definitions.types = GetTypes(table);
		definitions.functions = GetFunctions(table);
		definitions.functionsGeneratedAsNested = GetFunctionsGeneratedAsNested(o as Program);
		return definitions;
	}
	private Definitions.NList GetTypes(SymbolTable table) {
		var list = new Definitions.NList();
		if (table == null) return list;
		foreach(var items in table.Types) list.AddRange(items.Value);
		list.AddRange(GetTypes(table.EnclosingScope));
		return list;
	}
	private Definitions.NList GetFunctions(SymbolTable table) {
		var list = new Definitions.NList();
		if (table == null) return list;
		foreach(var items in table.Functions) list.AddRange(items.Value.Where(fd => !fd.GenerateAsNested));
		list.AddRange(GetFunctions(table.EnclosingScope));
		return list;
	}

    internal static Definitions.NList GetFunctionsGeneratedAsNested(Program pgm)
    {
        var list = new Definitions.NList();
        if (pgm == null) return list;

        list.AddRange(pgm.Children.OfType<ProcedureDivision>().SelectMany(c => c.Children)
            .Where(c => c is FunctionDeclaration fun && fun.GenerateAsNested));

        foreach (var pgmNestedProgram in pgm.NestedPrograms)
        {
            list.AddRange(GetFunctionsGeneratedAsNested(pgmNestedProgram));
        }
        return list;
    }

    }
    public class Definitions {
	public NList types;
	public NList functions;
	public NList functionsGeneratedAsNested;

	public override string ToString() {
		var str = new System.Text.StringBuilder();
		str.Append("Types:[");
		foreach(var item in types) str.Append(item.Name).Append(',');
		if (types.Count > 0) str.Length -= 1;
		str.Append("] Functions:[");
		foreach(var item in functions) str.Append(item.Name).Append(',');
		if (functions.Count > 0) str.Length -= 1;
		str.Append(']');
		return str.ToString();
	}

	public class NList: List<Node> {
		internal NList(): base() { }

        public List<Node> Public => this.FindAll(node => node is FunctionDeclaration fd && fd.CodeElement.Visibility == AccessModifier.Public);

        public List<Node> Private => this.FindAll(node => node is FunctionDeclaration fd && fd.CodeElement.Visibility != AccessModifier.Public);

        public IEnumerable<Node> Concat(List<Node> list, bool publicVisibility)
        {
            if (publicVisibility)
            {
                return this.Public.Concat(list);
            }

            return this.Private.Concat(list);
        }
    }
}

internal class VisibilityAttribute: Attribute {
	public object GetValue(object o, SymbolTable table) {
		var fun = o as FunctionDeclaration;
		if (fun != null) return fun.CodeElement.Visibility.ToString();
		return null;
	}
}

internal class LibraryCopyAttribute: Attribute {
	public object GetValue(object o, SymbolTable table) {
		var pgm = ((Node)o).GetProgramNode();

		var copies = pgm.GetChildren<LibraryCopy>();
		var copy = copies.Count > 0? copies[0] : null;
		return copy == null? "?TCRFUN_LIBRARY_COPY?" : copy.CodeElement.Name.Name;
	}
}
    /// <summary>
    /// return the name of enclosing program of the current node.
    /// The name is limited to 8 characters
    /// </summary>
    internal class ProgramName8Attribute: Attribute {
	    public object GetValue(object o, SymbolTable table) {
            var node = o as Node;
	        while (node != null) {
	            var pgm = node as Program;
	            if (pgm != null) {
	                var name = pgm.Name;
                    return pgm.Name.Substring(0,Math.Min(name.Length, 8));
	            }
	            node = node.Parent;
	        }
	        return "";
	    }
    }

    public class ProcedureImport
    {
        /// <summary>
        /// Hash of the Imported Procedure
        /// </summary>
        public string Hash
        { get; internal set; }

        /// <summary>
        /// Name of the Imported procedure
        /// </summary>
        public string Name
        { get; internal set; }

        public bool IsNotByExternalPointer
        {
            get
            {
                return ProcStyleCall.IsNotByExternalPointer;
            }
            set
            {
                ProcStyleCall.IsNotByExternalPointer = value;
            }
        }
        /// <summary>
        /// The target Procedure Style Call
        /// </summary>
        public ProcedureStyleCall ProcStyleCall
        {
            get;
            internal set;
        }
    }

    /// <summary>
    /// Descriptor of an Imoprted Program.
    /// </summary>
    public class ProgramImport
    {
        /// <summary>
        /// The Imported Program Name.
        /// </summary>
        public string Name
        { get; internal set; }

        /// <summary>
        /// List of All imported procedure from the Program (hash,name).
        /// </summary>
        public Dictionary<string, ProcedureImport> Procedures
        { get; internal set; }


        /// <summary>
        /// Determine if in this Imported program we uses Public Procedures.
        /// </summary>
        public bool HasPublicProcedures => Procedures.Count > 0;

        public ProgramImport()
        {
            Procedures = new Dictionary<string, ProcedureImport>();
        }
    }

    /// <summary>
    /// All Imports from Program.
    /// </summary>
    public class ProgramImports
    {
        /// <summary>
        /// Imported Programs
        /// </summary>
        public Dictionary<string, ProgramImport> Programs;
        /// <summary>
        /// Is The this Program Import Empty ?
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Programs.Count == 0;
            }
        }
        /// <summary>
        /// Is The this Program Import Not Empty ?
        /// </summary>
        public bool IsNotEmpty
        {
            get
            {
                return !IsEmpty;
            }
        }

        public bool HasPublicProcedures
        {
            get { return IsNotEmpty && Programs.Values.Any(prg => prg.HasPublicProcedures); }
        }
        public ProgramImports()
        {
            Programs = new Dictionary<string, ProgramImport>();
        }
    }

    /// <summary>
    /// return all imports of a Program Node
    /// </summary>
    public class ProgramImportsAttribute : Attribute {

        public static ProgramImports GetProgramImports([NotNull] IProcCaller procCaller) {
            ProgramImports imports = new ProgramImports();
            if (procCaller.ProcStyleCalls != null)
            {
                //All imported program.                    
                string name_low = procCaller.Name.ToLower();
                //For each entry in the Procedure Style Call Dictionary
                foreach (var e in procCaller.ProcStyleCalls)
                {
                    var hash = e.Key;
                    var call = e.Value;
                    ProcedureStyleCall proc_style_call = call.Item2;
                    //Only import Public Functions
                    FunctionDeclaration fun_decl = proc_style_call.FunctionDeclaration;
                    if (fun_decl != null)
                    {
                        if (fun_decl.CodeElement.Visibility != AccessModifier.Public)
                            continue;//Ignore a non-Public function ==> Cannot Import It.
                    }
                    var item_pgm = call.Item1[call.Item1.Count - 1];
                    if (name_low.Equals(item_pgm.Name.ToLower()))
                    {   //Avoid imports to itself.
                        continue;
                    }
                    string item_pgm_name = Hash.CalculateCobolProgramNameShortcut(item_pgm.Name);
                    string item_pgm_name_low = item_pgm_name.ToLower();
                    ProgramImport prg_imp = null;
                    if (!imports.Programs.ContainsKey(item_pgm_name_low))
                    {
                        prg_imp = new ProgramImport();
                        prg_imp.Name = item_pgm_name;
                        imports.Programs[item_pgm_name_low] = prg_imp;
                    }
                    if (prg_imp == null)
                        prg_imp = imports.Programs[item_pgm_name_low];
                    //Store the Procedure if it is not alreday there                                                
                    string proc_hash = hash;
                    if (!prg_imp.Procedures.ContainsKey(proc_hash))
                    {
                        ProcedureImport proc_imp = new ProcedureImport();
                        string item_proc_name = call.Item1[0].Name;
                        proc_imp.Name = item_proc_name;
                        proc_imp.Hash = proc_hash;
                        proc_imp.ProcStyleCall = proc_style_call;
                        prg_imp.Procedures[proc_hash] = proc_imp;
                    }
                }
            }
            return imports;
        }
        public object GetValue(object o, SymbolTable table)
        {
            
            var program = o as IProcCaller;
            if (program != null) {
                return GetProgramImports(program);
            }
            return null;
        }
    }
}
