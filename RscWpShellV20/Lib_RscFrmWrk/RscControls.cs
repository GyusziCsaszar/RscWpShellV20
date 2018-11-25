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

namespace Ressive.FrameWork
{

	
	public class TextBoxDenieEdit : TextBox
	{
		
		Button m_btnShield;
		
		public TextBoxDenieEdit(bool bMultiLine, bool bWordWrap, Grid grd, DependencyProperty dp, object dv)
		{
			Margin = new Thickness(-12, -12, -12, -12);
			IsReadOnly = false;
			TextAlignment = TextAlignment.Left;
			if( bMultiLine ) AcceptsReturn = true;
			if( bWordWrap ) TextWrapping = TextWrapping.Wrap;
			BorderThickness = new Thickness(0);
			SetValue(dp, dv);
			grd.Children.Add(this);
			TabIndex = -1;
			
			m_btnShield = new Button();
			m_btnShield.Margin = new Thickness(-12, -12, -12, -12);
			m_btnShield.BorderThickness = new Thickness(0);
			m_btnShield.Opacity = 0;
			m_btnShield.SetValue(dp, dv);
			grd.Children.Add(m_btnShield);
			
			m_btnShield.Tag = this;
		}
		
		new public System.Windows.Visibility Visibility
		{
			set{ base.Visibility = value; m_btnShield.Visibility = value; }
			get{ return base.Visibility; }
		}
		
		public Button ButtonShield
		{
			get
			{
				m_btnShield.Opacity = 0.5;
				return m_btnShield;
			}
		}
		
		public Thickness MarginOffset
		{
			set
			{
				Margin = new Thickness( Margin.Left + value.Left, Margin.Top + value.Top,
					Margin.Right + value.Right, Margin.Bottom + value.Bottom );
			}
		}
		
	}
	
	public class RscIcon
	{
		
		Image m_img;
		
		public RscIcon( Grid grd, DependencyProperty dp, object dv, int iCX, int iCY, System.Windows.Visibility Vis )
		{
			m_img = new Image();
			m_img.Visibility = Vis;
			
			//TODO... ...is this good for all?
			m_img.Stretch = Stretch.Uniform;

			m_img.Width = iCX;
			m_img.Height = iCY;
			m_img.Margin = new Thickness(2);
			m_img.SetValue(dp, dv);
			grd.Children.Add(m_img);
		}
		
		public Image Image
		{
			get{ return m_img; }
		}
		
	}
	
	public class RscIconButton : Button
	{
		
		Image m_img;
		
		public RscIconButton( Grid grd, DependencyProperty dp, object dv, int iCX, int iCY, System.Windows.Visibility Vis, int iMarginX = 0, int iMarginY = 0, string sBtnTitle = "" )
		{
			
			m_img = new Image();
			m_img.Visibility = Vis;
			
			if( sBtnTitle.Length > 0 )
			{
				//m_img.Stretch = Stretch.Uniform;
				m_img.Stretch = Stretch.Fill;
			}
			else
			{
				m_img.Width = iCX;
			}
			
			m_img.Height = iCY;
			m_img.Margin = new Thickness(iMarginX + 2, iMarginY + 2, 2, 2);
			m_img.SetValue(dp, dv);
			grd.Children.Add(m_img);

			Visibility = Vis;
			Margin = new Thickness(iMarginX + -12, iMarginY + -12, -12, -12);
			BorderThickness = new Thickness(0);
			
			if( sBtnTitle.Length > 0 )
			{
				Foreground = new SolidColorBrush( Colors.Black ); //Colors.Blue );
				Content = sBtnTitle;
				FontSize = 16;
				
				m_img.Opacity = 0.8;
				
				//ROLLBACK: Endless loop...
				/*
				SizeChanged += new System.Windows.SizeChangedEventHandler(My_SizeChanged);
				*/
			}
			else
			{
				Opacity = 0.5;
			}
			
			SetValue(dp, dv);
			grd.Children.Add(this);
		}
		
		//ROLLBACK: Endless loop...
		/*
		private void My_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			int iWidth = (int) (e.NewSize.Width);
			
			if( m_img.Width != iWidth )
			{
				m_img.Width = iWidth;
			}
		}
		*/
		
		public Image Image
		{
			get{ return m_img; }
		}
		
		new public System.Windows.Visibility Visibility
		{
			set{ base.Visibility = value; m_img.Visibility = value; }
			get{ return base.Visibility; }
		}
		
		public Thickness MarginOffset
		{
			set
			{
				m_img.Margin = new Thickness( m_img.Margin.Left + value.Left, m_img.Margin.Top + value.Top,
					m_img.Margin.Right + value.Right, m_img.Margin.Bottom + value.Bottom );
				
				Margin = new Thickness( Margin.Left + value.Left, Margin.Top + value.Top,
					Margin.Right + value.Right, Margin.Bottom + value.Bottom );
			}
		}
		
	}
	
}