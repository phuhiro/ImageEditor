﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Diagnostics;
using System.Drawing.Imaging;
using imageeditor.Model;
using imageeditor.Util;
using System.Threading;
using AutoArtist.Model;
using imageeditor.Properties;

namespace imageeditor.Controller
{
    public class ArtistController : AutoArtist.Model.Singleton<ArtistController>
    {
        private AutoArtist.Model.ArtistProps props;
        private StringFormat stringFormat;
        private AutoArtist.Model.LayerManager layerManager;

        public ArtistController()
        {
            stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap;
            stringFormat.Trimming = StringTrimming.Character;
            
            layerManager = new AutoArtist.Model.LayerManager();
            layerManager.ZoomPercent = 0.15f;
        }


        private Font GetCustomFont(String path, int size)
        {
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(path);
            return new Font(pfc.Families[0], size, FontStyle.Regular, GraphicsUnit.Pixel);
        }


        private void Init(string logoName)
        {
            props = new AutoArtist.Model.ArtistProps();
            props.StringFormat = stringFormat;
            props.LogoRootPath = FileUtil.GetCurPath(Settings.Default.LogoPath + logoName + "\\");
            props.ScriptConfig = FileUtil.ReadConfig<ScriptConfig>(props.LogoRootPath + "script.xml");
            props.LogoTypeName = logoName;

            layerManager.RemoveAllLayer();
            layerManager.AddLayer(props.LogoRootPath + "logo.png");
        }

        public void DrawByName(string name, string exportPath, string logoName)
        {
            try
            {
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }               

                // get list logo order behind main logo
                ScriptConfig config = props.ScriptConfig;
                List<ScriptObject> logoList = config.LogoList.OrderBy(p => p.Order).ToList();
                string[] names = name.Trim().Split(new string[]{Settings.Default.SplitString}, StringSplitOptions.None);
                string exportName = name.Replace(Settings.Default.SplitString, Settings.Default.ExportSplitString);
                for (int i = 0; i <names.Length ; i++)
                {
                    ScriptObject obj = logoList[i];
                    string fontPath = props.LogoRootPath + obj.FontName;
                    Font ff = GetCustomFont(fontPath, obj.FontSize);
                    {
                        AutoArtist.Model.Layer layer = new AutoArtist.Model.Layer(new AutoArtist.Model.LayerText(names[i], ff, obj.FontSize, LogoUtil.ConvertStringToColor(obj.TextColor)
                            , (int)obj.OutlineSize, LogoUtil.ConvertStringToColor(obj.OutlineColorHex)
                            , (LineJoin)obj.LineJoin));

                        layer.UpdateLayer(obj.LogoPosition, obj.LogoScale, obj.LogoRotation, obj.LogoContainer);
                        layerManager.AddTextLayer(layer);
                    }                  

                } 
                if(logoList.Count > 0)
                {
                    layerManager.ExportImage(exportPath + exportName + ".png", new Size(2400, 3200), config.AutoScaleAll);
                }else
                {
                    System.Windows.Forms.MessageBox.Show("Cannot found any logo info!");
                }
                
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void ExportLogo(string name, string exportPath, string logoName)
        {
            Init(logoName);
            if (exportPath[exportPath.Length - 1] != '\\')
                exportPath += "\\";
            DrawByName(name, exportPath, logoName);
        }

    }
}
