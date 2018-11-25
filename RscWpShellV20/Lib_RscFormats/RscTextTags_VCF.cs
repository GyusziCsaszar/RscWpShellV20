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

using Ressive.Utils;

namespace Ressive.Formats
{
	
	public class RscTextTags_VCF : RscTextTags
	{
		
		public string Name = "";
		public string FullName = "";
		public string Class = "";
		
		protected List<RscTextTags_VCF_PhoneNumber> m_aPhoneNumbers = new List<RscTextTags_VCF_PhoneNumber>();
		
		protected int PhotoStart = -1;
		protected int PhotoEnd = -1;
		
		public RscTextTags_VCF()
		{
		}
		
		public string PhoneNumber( int iIndex )
		{
			if( iIndex >= m_aPhoneNumbers.Count ) return "";
			return m_aPhoneNumbers[ iIndex ].Number;
		}
		
		public bool PhotoPresent{ get{ return (PhotoStart >= 0) && (PhotoEnd >= 0); } }
		public bool PhotoIsBase64{ get{ return PhotoIs( "ENCODING=BASE64" ); } }
		public bool PhotoIs( string sLeftSub )
		{
			if( PhotoStart < 0 ) return false;
			if( PhotoEnd < 0 ) return false;
			
			return (m_aTags[ PhotoStart ].LeftIndexOf( sLeftSub ) >= 0);
		}
		public string PhotoData
		{
			get
			{
				
				if( PhotoStart < 0 ) return "";
				if( PhotoEnd < 0 ) return "";
				
				string s = "";
				
				for( int i = PhotoStart; i <= PhotoEnd; i++ )
				{
					RscTextTag tt = m_aTags[ i ];
					if( i == PhotoStart )
						s += tt.RightSide;
					else
						s += "\r\n" + tt.LeftSide;
				}
				
				return s;
			}
		}
		
		public override string ToString()
		{
			string sRet = "";
			
			if( Name.Length > 0 )
			{
				sRet += "Name: " + Name;
				sRet += "\r\n";
			}
			
			if( FullName.Length > 0 )
			{
				sRet += "Full Name: " + FullName;
				sRet += "\r\n";
			}
			
			if( Class.Length > 0 )
			{
				sRet += "Class: " + Class;
				sRet += "\r\n";
			}
			
			foreach( RscTextTags_VCF_PhoneNumber pn in m_aPhoneNumbers )
			{
				sRet += "Phonenumber";
				if( pn.Kind.Length > 0 ) sRet += "(" + pn.Kind + ")";
				sRet += ": " + pn.Number;
				sRet += "\r\n";
			}
			
			if( PhotoStart >= 0 )
			{
				//sRet += "Image Present: ";
				//sRet += "Yes";
				sRet += "(photo present)";
			}
			else
			{
				//sRet += "Image Present: ";
				//sRet += "No";
			}
			sRet += "\r\n";
			
			if( LastError.Length > 0 )
			{
				sRet += "\r\n";
				sRet += base.ToString();
			}
			
			//DeBug...
			/*
			else
			{
				sRet += "\r\n";
				sRet += base.ToString();
			}
			*/
			
			return sRet;
		}
		
		public override void Parse( string sTxt, string sTagDelimiter, string sTagDataDelimiter, string sTagSubDataDelimiter )
		{
			
			Name = "";
			FullName = "";
			Class = "";
			m_aPhoneNumbers.Clear();
			PhotoStart = -1;
			PhotoEnd = -1;
			
			//FIX(es) for known case(es)...
			sTxt = sTxt.Replace( "=\r\n=", "=" );
			
			base.Parse( sTxt, sTagDelimiter, sTagDataDelimiter, sTagSubDataDelimiter );
			
			RscTextTag tt;
			
			int iCount = m_aTags.Count;
			for( int i = 0; i < iCount; i++ )
			{
				tt = m_aTags[ i ];
				
				switch( i )
				{
					
					case 0 :
					{
						if( !tt.ChkAgains( "begin", "vcard" ) )
						{
							LastErrorTagIndex = i;
							LastError = "Unexpected tag (name / value)!";
							return;
						}
						break;
					}
					
					case 1 :
					{
						if( !tt.ChkAgains( "version", "2.1" ) )
						{
							LastErrorTagIndex = i;
							LastError = "Unexpected tag (name / value)!";
							return;
						}
						break;
					}
					
					default :
					{
						if( i == m_aTags.Count - 1 )
						{
							if( tt.RightSide.Length == 0 )
							{
								if( PhotoStart >= 0 && PhotoEnd == -1 )
								{
									PhotoEnd = i;
								}
							}
										
							if( !tt.ChkAgains( "end", "vcard" ) )
							{
								LastErrorTagIndex = i;
								LastError = "Unexpected tag (name / value)!";
								return;
							}
							
							if( PhotoStart >= 0 && PhotoEnd == -1 )
							{
								PhotoEnd = i - 1;
							}
						}
						else
						{
							
							switch( tt.ID )
							{
								
								case "N" :
									Name = DeCodeValue(tt);
									break;
								
								case "FN" :
									FullName = DeCodeValue(tt);
									break;
									
								case "TEL" :
									RscTextTags_VCF_PhoneNumber pn = new RscTextTags_VCF_PhoneNumber();
									m_aPhoneNumbers.Add( pn );
									pn.Kind = tt.LeftSideSub( 1 );
									string sPn = tt.RightSide;
									sPn = sPn.Replace( "-", "" );
									pn.Number = sPn;
									break;
									
								case "X-CLASS" :
									Class = tt.RightSide;
									break;
									
								case "PHOTO" :
									if( PhotoStart >= 0 )
									{
										LastErrorTagIndex = i;
										LastError = "Multiple photos not supported!";
										return;
									}
									PhotoStart = i;
									break;
								
								default :
								{
									bool bOk = false;
									
									if( tt.RightSide.Length == 0 )
									{
										if( PhotoStart >= 0 && PhotoEnd == -1 )
										{
											bOk = true;
										}
									}
									else
									{
										if( PhotoStart >= 0 && PhotoEnd == -1 )
										{
											PhotoEnd = i - 1;
										}
									}
									
									if( !bOk )
									{
										LastErrorTagIndex = i;
										LastError = "Unrecognized tag '" + tt.ID + "'!";
										return;
									}
									
									break;
								}
								
							}
							
						}
						break;
					}
						
				}
				
			}
		}
		
		protected string DeCodeValue( RscTextTag tt )
		{
			string sRet = "";
			
			int iCount = tt.m_asRight.Length;
			for( int i = 0; i < iCount; i++ )
			{
				string sVal = "";
				
				sVal = tt.m_asRight[ i ];
				
				if( tt.LeftIndexOf( "ENCODING=QUOTED-PRINTABLE" ) >= 0 )
				{
					sVal = RscDecode.QuotedPrintable( sVal );
				}
				
				if( tt.LeftIndexOf( "ENCODING=8BIT" ) >= 0 )
				{
					//NOP...
				}
				else
				{
					if( tt.LeftIndexOf( "CHARSET=UTF-8" ) >= 0 )
					{
						if( sVal.Length > 0 )
						{
							char [] acVal = sVal.ToCharArray();
							byte [] ay = new byte [ acVal.Length ];
							for( int j = 0; j < acVal.Length; j++ )
								ay[ j ] = ((byte) (acVal[ j ]));
							sVal = Encoding.UTF8.GetString(ay, 0, ay.Length);
						}
					}
				}
				
				if( sVal.Length > 0 )
				{
					if( sRet.Length > 0 ) sRet += " ";
					sRet += sVal;
				}
			}
			
			return sRet;
		}
		
	}
	
	public class RscTextTags_VCF_PhoneNumber
	{
		public string Kind {set;get;}
		public string Number {set;get;}
		public RscTextTags_VCF_PhoneNumber()
		{
			Kind = "";
			Number = "";
		}
	}
	
}