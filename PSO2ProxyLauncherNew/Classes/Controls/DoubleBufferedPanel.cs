﻿using PSO2ProxyLauncherNew.Classes.Components;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PSO2ProxyLauncherNew.Classes.Controls
{
    class DoubleBufferedPanel : Panel
    {
        //Components.DirectBitmap innerbuffer, innerbgbuffer;

        public DoubleBufferedPanel() : base()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
            this.DoubleBuffered = true;
            this._CacheBackground = false;
            this.UpdateStyles();
        }

        private bool _CacheBackground;
        public bool CacheBackground
        {
            get { return this._CacheBackground; }
            set
            {
                this._CacheBackground = value;
                this.GetNewCache();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.GetNewCache();
        }

        protected override void OnParentBackgroundImageChanged(EventArgs e)
        {
            base.OnParentBackgroundImageChanged(e);
            this.GetNewCache();
        }

        public void GetNewCache()
        {
            if (this.CacheBackground)
            {
                this.BackgroundImage = null;
                if (myBGCache != null)
                    myBGCache.Dispose();
                myBGCache = new DirectBitmap(this.Width, this.Height);
                ButtonRenderer.DrawParentBackground(myBGCache.Graphics, this.ClientRectangle, this);
                this.BackgroundImage = myBGCache.Bitmap;
            }
        }

        public new void Dispose()
        {
            base.Dispose();
            if (this.myBGCache != null)
                myBGCache.Dispose();
        }

        private DirectBitmap myBGCache;
    }
}
