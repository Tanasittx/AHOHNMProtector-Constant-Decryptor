/*
 * Created by SharpDevelop.
 * User: Mihai
 * Date: 5/7/2015
 * Time: 2:51 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.DotNet.Emit;

namespace ConfuserExConstantDecryptor
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void TextBox1DragDrop(object sender, DragEventArgs e)
		{
		try
        { 
        Array a = (Array) e.Data.GetData(DataFormats.FileDrop);
        if(a != null)
        {
        string s = a.GetValue(0).ToString();
        int lastoffsetpoint = s.LastIndexOf(".");
        if (lastoffsetpoint != -1)
        {
        string Extension = s.Substring(lastoffsetpoint);
        Extension=Extension.ToLower();
        if (Extension == ".exe"||Extension == ".dll")
        {
        this.Activate();
        textBox1.Text = s;
        int lastslash = s.LastIndexOf("\\");
        if (lastslash!=-1) DirectoryName = s.Remove(lastslash,s.Length-lastslash);
        if (DirectoryName.Length==2) DirectoryName=DirectoryName+"\\";
        }
        }
        }
        }
        catch
        {

        }
		}
		
		void TextBox1DragEnter(object sender, DragEventArgs e)
		{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.Copy;
    	else
    	e.Effect = DragDropEffects.None;
		}
		
		public string DirectoryName = "";
		void Button1Click(object sender, EventArgs e)
		{
		label2.Text="";
		OpenFileDialog fdlg = new OpenFileDialog();
		fdlg.Title = "Browse for target assembly";
		fdlg.InitialDirectory = @"c:\";
		if (DirectoryName!="") fdlg.InitialDirectory = DirectoryName;
		fdlg.Filter = "All files (*.exe,*.dll)|*.exe;*.dll";
		fdlg.FilterIndex = 2;
		fdlg.RestoreDirectory = true;
		if(fdlg.ShowDialog() == DialogResult.OK)
		{
		string FileName = fdlg.FileName;
		textBox1.Text = FileName;
		int lastslash = FileName.LastIndexOf("\\");
		if (lastslash!=-1) DirectoryName = FileName.Remove(lastslash,FileName.Length-lastslash);
        if (DirectoryName.Length==2) DirectoryName=DirectoryName+"\\";
		}
		}
		
		void Button3Click(object sender, EventArgs e)
		{
		Application.Exit();
		}
		
		List<MethodDef> methods = new List<MethodDef>();
		
		public void AddMethods(TypeDef type)
		{
		if (type.HasMethods)
		{
		        foreach (MethodDef method in type.Methods)
                {
                if (method.HasBody)
                methods.Add(method);
                }
		}
		    if (type.HasNestedTypes)
            {
		    	foreach (TypeDef nestedtype in type.NestedTypes)
                {
                AddMethods(nestedtype);
                }
		    }
		}
		
public MethodInfo GetStringMethod(int MethodToken)
{
    MethodInfo info = (MethodInfo) assembly.ManifestModule.ResolveMethod(MethodToken);
    if (info.IsGenericMethodDefinition || info.IsGenericMethod)
    {
        return info.MakeGenericMethod(new Type[] { typeof(string) });
    }
    return info;
}
	
public MethodInfo GetIntMethod(int MethodToken)
{
    MethodInfo info = (MethodInfo) assembly.ManifestModule.ResolveMethod(MethodToken);
    if (info.IsGenericMethodDefinition || info.IsGenericMethod)
    {
        return info.MakeGenericMethod(new Type[] { typeof(int) });
    }
    return info;
}

public MethodInfo GetFloatMethod(int MethodToken)
{
    MethodInfo info = (MethodInfo) assembly.ManifestModule.ResolveMethod(MethodToken);
    if (info.IsGenericMethodDefinition || info.IsGenericMethod)
    {
        return info.MakeGenericMethod(new Type[] { typeof(float) });
    }
    return info;
}
		Assembly assembly;
		void Button2Click(object sender, EventArgs e)
		{
	if (File.Exists(textBox1.Text))
	{
AssemblyName an = AssemblyName.GetAssemblyName(textBox1.Text);
assembly = Assembly.Load(an);
if (assembly==null)
{
MessageBox.Show("Failed to load the assembly! Aborting!");
return;
}
string directoryname = Path.GetDirectoryName(textBox1.Text);
if (!directoryname.EndsWith("\\")) directoryname=directoryname+"\\";
string newfilename = directoryname+Path.GetFileNameWithoutExtension(textBox1.Text)+"_constantsdec"+Path.GetExtension(textBox1.Text);

	// Open or create an assembly
	 AssemblyDef asm = AssemblyDef.Load(textBox1.Text);
	 ModuleDef mod = asm.ManifestModule;
	if (mod.IsILOnly)
	{
    ModuleWriterOptions writerOptions = new ModuleWriterOptions(mod);
    writerOptions.MetaDataOptions.Flags |= MetaDataFlags.PreserveUSOffsets | MetaDataFlags.PreserveBlobOffsets | MetaDataFlags.PreserveExtraSignatureData | MetaDataFlags.PreserveAllMethodRids | MetaDataFlags.PreserveFieldRids | MetaDataFlags.PreserveParamRids | MetaDataFlags.PreserveTypeRefRids | MetaDataFlags.PreserveTypeDefRids | MetaDataFlags.PreservePropertyRids | MetaDataFlags.PreserveTypeSpecRids | MetaDataFlags.PreserveStandAloneSigRids | MetaDataFlags.PreserveEventRids | MetaDataFlags.KeepOldMaxStack;
	methods = new List<MethodDef>();
	if (mod.HasTypes)
	{
    foreach (TypeDef typedef in mod.Types)
    {
	AddMethods(typedef);
    }
	}
	
	int replacecount = 0;
	for (int i = 0; i < methods.Count; i++) // Loop with for.
	{  // search on all methods for strings:
	for (int j = 0x0; j < methods[i].Body.Instructions.Count; j++)
    {
	if (methods[i].Body.Instructions[j].OpCode == OpCodes.Call && 
		  methods[i].Body.Instructions[j].Operand.ToString().Contains("String>")
		  &&(j-01)>=0&&methods[i].Body.Instructions[j - 0x1].Operand.ToString() == "Encrypted By Nightbaron"
        && (j-02)>=0&&methods[i].Body.Instructions[j - 0x2].IsLdcI4()  )


        {
                                MethodInfo method = null;
                                object returnedstring = null;
                            
                                MessageBox.Show("Found String!");
                                try
                                {
                                    if (methods[i].Body.Instructions[j].Operand.GetType().ToString().Contains("MethodDef"))
                                    {
                                        method = GetStringMethod((int) ((MethodDef) methods[i].Body.Instructions[j].Operand).MDToken.Raw);
                                    }
                                    else                              
                                    {
                                        method = GetStringMethod((int) ((MethodSpec) methods[i].Body.Instructions[j].Operand).Method.MDToken.Raw);
                                    }
                                    string stringtext1 = methods[i].Body.Instructions[j - 0x1].Operand.ToString();
                                    int intergerevalue1 = methods[i].Body.Instructions[j - 0x2].GetLdcI4Value();
                                    uint intergerevalue2 = (uint)methods[i].Body.Instructions[j - 0x3].GetLdcI4Value();
                                    int intergerevalue3 = methods[i].Body.Instructions[j - 0x4].GetLdcI4Value();
                                    uint intergerevalue4 = (uint)methods[i].Body.Instructions[j - 0x5].GetLdcI4Value();
                                    returnedstring = method.Invoke(null, new object[] { intergerevalue4,intergerevalue3, intergerevalue2, intergerevalue1,stringtext1 });
                                    MessageBox.Show(returnedstring.ToString());
                                }
                                catch (Exception exc)
                                {
                                    int whatthehell = 1;
                                }
                                if (returnedstring != null)
                                {
                                    methods[i].Body.Instructions[j - 0x1].OpCode = OpCodes.Nop;
                                    methods[i].Body.Instructions[j - 0x2].OpCode = OpCodes.Nop;
                                    methods[i].Body.Instructions[j - 0x3].OpCode = OpCodes.Nop;
                                    methods[i].Body.Instructions[j - 0x4].OpCode = OpCodes.Nop;
                                    methods[i].Body.Instructions[j - 0x5].OpCode = OpCodes.Nop;
                                    methods[i].Body.Instructions[j].OpCode = OpCodes.Ldstr;
                                    methods[i].Body.Instructions[j].Operand = (string) returnedstring;
                                    replacecount++;
                                }

                            } // end of string decryption

		
		
		//if (methods[i].Body.Instructions[j].OpCode == OpCodes.Call && 
		//  methods[i].Body.Instructions[j].Operand.ToString().Contains("Int32>")
		//  &&(j-01)>=0&&methods[i].Body.Instructions[j - 0x1].IsLdcI4()
  //        && (j - 02) >= 0 && methods[i].Body.Instructions[j - 0x2].IsLdcI4())
  //      {
		//	MethodInfo method = null;
  //          object returnedint = null;
  //          try
  //          {

  //              if (methods[i].Body.Instructions[j].Operand.GetType().ToString().Contains("MethodDef"))
  //              {
  //                  method = GetIntMethod((int) ((MethodDef) methods[i].Body.Instructions[j].Operand).MDToken.Raw);
  //              }
  //              else
  //              {
  //                  method = GetIntMethod((int) ((MethodSpec) methods[i].Body.Instructions[j].Operand).Method.MDToken.Raw);
  //              }
  //                                  uint intergerevalue1 = (uint)methods[i].Body.Instructions[j - 0x1].GetLdcI4Value();
  //                                  uint intergerevalue2 = (uint)methods[i].Body.Instructions[j - 0x2].GetLdcI4Value();
  //                                  returnedint = method.Invoke(null, new object[] { intergerevalue2, intergerevalue1 });
  //                              }
  //          catch
  //          {
  //          }
            
  //          if (returnedint != null)
  //          {
  //                                  methods[i].Body.Instructions[j - 0x1].OpCode = OpCodes.Nop;
  //                                  methods[i].Body.Instructions[j - 0x2].OpCode = OpCodes.Nop;
  //                                  methods[i].Body.Instructions[j].OpCode = OpCodes.Ldc_I4;
  //              methods[i].Body.Instructions[j].Operand = (int) returnedint;
  //              replacecount++;
  //          }


	 // } // end of int decryption 
		
		
		
		//	if (methods[i].Body.Instructions[j].OpCode == OpCodes.Call && 
		//  methods[i].Body.Instructions[j].Operand.ToString().Contains("Single>")
		//  &&(j-01)>=0&&methods[i].Body.Instructions[j - 0x1].IsLdcI4()
  //        && (j - 02) >= 0 && methods[i].Body.Instructions[j - 0x2].IsLdcI4())
  //      {
		//	MethodInfo method = null;
  //          object returnedfloat = null;
  //          try
  //          {

  //              if (methods[i].Body.Instructions[j].Operand.GetType().ToString().Contains("MethodDef"))
  //              {
  //                  method = GetFloatMethod((int) ((MethodDef) methods[i].Body.Instructions[j].Operand).MDToken.Raw);
  //              }
  //              else
  //              {
  //                  method = GetFloatMethod((int) ((MethodSpec) methods[i].Body.Instructions[j].Operand).Method.MDToken.Raw);
  //              }
  //                                  uint intergerevalue1 = (uint)methods[i].Body.Instructions[j - 0x1].GetLdcI4Value();
  //                                  uint intergerevalue2 = (uint)methods[i].Body.Instructions[j - 0x2].GetLdcI4Value();
  //                                  returnedfloat = method.Invoke(null, new object[] { intergerevalue2, intergerevalue1 });
  //                              }
  //          catch
  //          {
  //          }
            
  //          if (returnedfloat != null)
  //          {
  //                                  methods[i].Body.Instructions[j - 0x1].OpCode = OpCodes.Nop;
  //                                  methods[i].Body.Instructions[j - 0x2].OpCode = OpCodes.Nop;
  //                                  methods[i].Body.Instructions[j].OpCode = OpCodes.Ldc_R4;
  //              methods[i].Body.Instructions[j].Operand = (float) returnedfloat;
  //              replacecount++;
  //          }


	 // } // end of float decryption */
		
	}
	}

                    writerOptions.Logger = DummyLogger.NoThrowInstance;
                     mod.Write(newfilename, writerOptions);
                    label2.Text = replacecount.ToString()+" constants decrypted!";
    
	}
	}
		}
	}
}
