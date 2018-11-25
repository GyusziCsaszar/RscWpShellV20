using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ressive.Formats
{
	
	public static class RscJSon
	{
		
		public static string UnDecorate( string s )
		{
			
			s = s.Replace( "&lt;", "<" );
			s = s.Replace( "&gt;", ">" );
			
			s = s.Replace( "&#39;", "'" );
			
			return s;
		}
		
		const int ciLogSampleLenHalf = 40;
		
		public static RscJSonItem FromResponseContetn( string s, out string sErr, string sID = "", string sDescription = "" )
		{
			return RscJSon.FromResponseContetn( null, s, out sErr, sID, sDescription );
		}
		
		public static RscJSonItem FromResponseContetn( RscJSonItem jsonRoot, string s, out string sErr, string sID = "", string sDescription = "" )
		{
			sErr = "";
			
			//RscJSonItem jsonRoot = null;
			RscJSonItem json = jsonRoot; //null;
			
			if( jsonRoot != null )
			{
				jsonRoot.ID = sID;
				jsonRoot.Description = sDescription;
			}
			
			int iLen = s.Length;
			
			int iLevel = 0;
			int iPosString = -1;
			string sName = "";
			bool bValue = false;
			string sStripValue = "";
			
			int iPos = -1;
			for(;;)
			{
				iPos++;
				if( iPos >= iLen )
				{
					break;
				}
				
				if( iPosString >= 0 )
				{
					if( s[ iPos ] != '"' )
						continue;
					
					if( iPos > 0 )
					{
						//This can be occure: "abc\"def"
						if( s[ iPos - 1 ] == '\\' )
							continue;
					}
				}
				
				switch( s[ iPos ] )
				{
					
					case '{' :
					{
						//FIX: { always inces level!!!
						/*
						if( bValue )
						{
							//
							// FIX: Array can be started with { not only [!!!
							//
							/*
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
							*
							
							//Array start...
							if( json == null )
							{
								RscJSonItem jsonRet = null;
								sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
								return jsonRet;
							}
							
							json.Name = sName;
							
							bValue = false;
						}
						else
						*/
						{						
							iLevel++;
							
							if( jsonRoot == null )
							{
								jsonRoot = new RscJSonItem();
								
								jsonRoot.ID = sID;
								jsonRoot.Description = sDescription;
								
								json = jsonRoot;
							}
							else
							{
								RscJSonItem jsonNew = new RscJSonItem( json );
								json.Add( jsonNew );
								json = jsonNew;
							}
							
							//FIX: { always inces level!!!
							if( bValue )
							{
								json.Name = sName;
								
								bValue = false;
							}
						}
						
						break;
					}
					
					case '}' :
					{
						if( bValue )
						{
							if( sStripValue.Length > 0 )
							{
								bValue = false;
								
								json.AddProperty( sName, sStripValue );
								
								//FIX: Not to reuse...
								sName = "";
								
								sStripValue = "";
							}
							else
							{
								RscJSonItem jsonRet = null;
								sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
								return jsonRet;
							}
						}
						else if( sStripValue.Length > 0 )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
						
						if( iLevel <= 0 )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
						
						iLevel--;
						json = json.Parent;
						
						break;
					}
					
					case '"' :
					{
						if( iPosString >= 0 )
						{
							if( json == null )
							{
								RscJSonItem jsonRet = null;
								sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
								return jsonRet;
							}
							
							if( bValue )
							{
								bValue = false;
								
								json.AddProperty( sName, s.Substring( iPosString + 1, Math.Max( 0, (iPos - iPosString) - 1) ) );
								
								//FIX: Not to reuse...
								sName = "";
							}
							else
							{
								sName = s.Substring( iPosString + 1, Math.Max( 0, (iPos - iPosString) - 1) );
							}
							
							iPosString = -1;
						}
						else
						{
							iPosString = iPos;
						}
						
						break;
					}
					
					case ':' :
					{
						if( bValue )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
						
						bValue = true;
						
						break;
					}
					
					case '[' :
					{
						//Array start...
						if( json == null )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
							
						if( !bValue )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
						
						//FIX: Handling ValueLists...
						/*
						json.Name = sName;
						
						bValue = false;
						*/
						iLevel++;
						
						if( jsonRoot == null )
						{
							jsonRoot = new RscJSonItem();
							
							jsonRoot.ID = sID;
							jsonRoot.Description = sDescription;
							
							json = jsonRoot;
						}
						else
						{
							RscJSonItem jsonNew = new RscJSonItem( json );
							json.Add( jsonNew );
							json = jsonNew;
						}
						
						json.Name = sName;
						
						bValue = false;

						break;
					}
					
					case ']' :
					{
						//Array end...
						
						if( bValue )
						{
							if( sStripValue.Length > 0 )
							{
								bValue = false;
								
								json.AddProperty( sName, sStripValue );
								
								//FIX: Not to reuse...
								sName = "";
								
								sStripValue = "";
							}
							else
							{
								RscJSonItem jsonRet = null;
								sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
								return jsonRet;
							}
						}
						else if( sStripValue.Length > 0 )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
						
						//FIX: Last Nameless value in array...
						if( !bValue )
						{
							if( sName.Length > 0 ) //Otherwise already done...
							{
								json.AddProperty( "", sName );
								
								//FIX: Not to reuse...
								sName = "";
							}
						}
						
						if( iLevel <= 0 )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
						
						iLevel--;
						json = json.Parent;
						
						break;
					}
					
					case ',' :
					{
						if( bValue )
						{
							if( sStripValue.Length > 0 )
							{
								bValue = false;
								
								json.AddProperty( sName, sStripValue );
								
								//FIX: Not to reuse...
								sName = "";
								
								sStripValue = "";
							}
							else
							{
								RscJSonItem jsonRet = null;
								sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
								return jsonRet;
							}
						}
						else if( sStripValue.Length > 0 )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
						
						//FIX: Nameless value...
						if( !bValue )
						{
							if( sName.Length > 0 ) //Otherwise already done...
							{
								json.AddProperty( "", sName );
								
								//FIX: Not to reuse...
								sName = "";
							}
						}
						
						//New property...
						break;
					}
					
					case '\r' :
					case '\n' :
					case ' ' :
					{
						//Decoration...
						break;
					}
					
					default :
					{
						if( !bValue )
						{
							RscJSonItem jsonRet = null;
							sErr = _BuildErrMsg( s, iPos, jsonRoot, out jsonRet );
							return jsonRet;
						}
						
						//Numbers are not string delimited...
						sStripValue += s[ iPos ].ToString();
						
						break;
					}
						
				}
			}
			
			if( jsonRoot == null )
				sErr = "No item found!";
			
			if( iLevel > 0 )
				sErr = "Incomplete item tree!";
			
			return jsonRoot;
		}
		
		private static string _BuildErrMsg(string s, int iPos, RscJSonItem jsonRoot, out RscJSonItem jsonRet)
		{
			//To be able to see partial result!!!
			jsonRet = jsonRoot; //null
			
			string sErr = "";
			
			int iLen = s.Length;
			
			sErr = "Unexpected '" + s[ iPos ].ToString() + "' at " + (iPos + 1).ToString() + " of " + iLen.ToString() + "!";
			
			int iEnd = Math.Min( iPos + ciLogSampleLenHalf, iLen - 1 );
			int iStart = Math.Max( 0, iEnd - (ciLogSampleLenHalf * 2) );
			
			//sErr += "\n" + s.Substring( iStart, (iEnd - iStart) + 1);
			sErr += "\n" + s.Substring( iStart, ((iPos - 1) - iStart) + 1);
			sErr += "\n==> " + s[ iPos ].ToString() + " <==";
			sErr += "\n" + s.Substring( (iPos + 1), (iEnd - (iPos + 1)) + 1);
			
			return sErr;
		}
		
	}
	
	public class RscJSonItem
	{
		
		RscJSonItem m_oParent = null;
		
		string m_sID = "";
		string m_sDescription = "";
		
		string m_sName = "";
		
		List<RscJSonItem> m_aChildren = new List<RscJSonItem>();
		
		List<RscJSonItemProperty> m_aProperties = new List<RscJSonItemProperty>();
		
		public RscJSonItem()
		{
		}
		
		public RscJSonItem( RscJSonItem oParent )
		{
			m_oParent = oParent;
		}
		
		public RscJSonItem Parent
		{
			get
			{
				return m_oParent;
			}
		}
		
		public string ID
		{
			set
			{
				m_sID = value;
			}
			get
			{
				return m_sID;
			}
		}
		
		public string Description
		{
			set
			{
				m_sDescription = value;
			}
			get
			{
				return m_sDescription;
			}
		}
		
		public string Name
		{
			set
			{
				m_sName = value;
			}
			get
			{
				return m_sName;
			}
		}
		
		public void Add( RscJSonItem oChild )
		{
			m_aChildren.Add( oChild );
		}
		
		public int ChildCount
		{
			get
			{
				return m_aChildren.Count;
			}
		}
		
		public RscJSonItem GetChild( int iIndex )
		{
			return m_aChildren[ iIndex ];
		}
		
		public RscJSonItem GetChildByName( string sName )
		{
			foreach( RscJSonItem json in m_aChildren )
			{
				if( json.Name == sName )
					return json;
			}
			
			return null;
		}
		
		public void AddProperty( string sName, string sValue )
		{
			//Decoration...
			if( sName == "name" )
			{
				if( Name.Length == 0 )
				{
					Name = sValue;
				}
			}
			
			RscJSonItemProperty oProp = new RscJSonItemProperty(sName, sValue);			
			m_aProperties.Add( oProp );
		}
		
		public int PropertyCount
		{
			get
			{
				return m_aProperties.Count;
			}
		}
		
		public RscJSonItemProperty GetProperty( int iIndex )
		{
			return m_aProperties[ iIndex ];
		}
		
		public string GetPropertyValue( string sName, bool bRemoveDecoration = false )
		{
			for( int i = 0; i < m_aProperties.Count; i++ )
			{
				if( m_aProperties[ i ].Name == sName )
				{
					return m_aProperties[ i ].Value( bRemoveDecoration );
				}
			}
			
			return "";
		}
		
		public string GetChildPropertyValue( string sChildName, string sPropertyName, bool bRemoveDecoration = false )
		{
			RscJSonItem jsonChild = GetChildByName( sChildName );
			if( jsonChild == null )
				return "";
			
			for( int i = 0; i < jsonChild.PropertyCount; i++ )
			{
				if( jsonChild.GetProperty(i).Name == sPropertyName )
				{
					return jsonChild.GetProperty(i).Value( bRemoveDecoration );
				}
			}
			
			return "";
		}
		
		public string GetChildPropertyValue( int iChildIndex, string sPropertyName, bool bRemoveDecoration = false )
		{
			RscJSonItem jsonChild = GetChild( iChildIndex );
			if( jsonChild == null )
				return "";
			
			for( int i = 0; i < jsonChild.PropertyCount; i++ )
			{
				if( jsonChild.GetProperty(i).Name == sPropertyName )
				{
					return jsonChild.GetProperty(i).Value( bRemoveDecoration );
				}
			}
			
			return "";
		}
		
		public string ToDecoratedString( int iIndent = 0 )
		{
			StringBuilder sb = new StringBuilder();
			
			sb.Append( new String(' ', iIndent) + "ID: " + m_sID + "\n" );
			sb.Append( new String(' ', iIndent) + "name: " + m_sName + "\n" );
			
			if( m_aProperties.Count > 0 )
			{
				sb.Append( new String(' ', iIndent) + "  properties: \n" );
				foreach( RscJSonItemProperty oProp in m_aProperties )
				{
					sb.Append( new String(' ', iIndent) + "    " + oProp.Name + ": " + oProp.Value( false ) + "\n" );
				}
			}
			
			if( m_aChildren.Count > 0 )
			{
				sb.Append( new String(' ', iIndent) + "  children: \n" );
				foreach( RscJSonItem oItem in m_aChildren )
				{
					sb.Append( oItem.ToDecoratedString( iIndent + 2 + 2 ) );
				}
			}
			
			return sb.ToString();
		}
		
	}
	
	public class RscJSonItemProperty
	{
		
		protected string m_sName = "";
		protected string m_sValue = "";
		
		public RscJSonItemProperty(string sName, string sValue)
		{
			m_sName = sName;
			m_sValue = sValue;
		}
		
		public string Name { get{ return m_sName; } }
		
		//public string Value { get{ return m_sValue; } }
		public string Value( bool bRemoveDecoration )
		{
			if( !bRemoveDecoration )
				return m_sValue;
			
			return RscJSon.UnDecorate( m_sValue );
		}
		
	}
	
}