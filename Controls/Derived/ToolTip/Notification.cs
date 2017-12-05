using Extensions;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Controls.Derived.ToolTip
{
    public class Notification : TimedToolTip
    {
        public Notification(UIElement placementTarget, Icon icon, string text)
            : base(placementTarget)
        {
            this.Content = new NotificationContent(icon, text);
            this.Placement = PlacementMode.Top;
            this.VerticalOffset = 2;
            this.HorizontalOffset = 20;
        }

        public Icon Icon
        {
            get
            {
                return (Content as NotificationContent).Icon;
            }
            set
            {
                (Content as NotificationContent).Icon = value;
            }
        }

        public double? IconWidth
        {
            get
            {
                return (Content as NotificationContent).IconWidth;
            }
            set
            {
                (Content as NotificationContent).IconWidth = value;
            }
        }

        public string Text
        {
            get
            {
                return (Content as NotificationContent).Text;
            }
            set
            {
                (Content as NotificationContent).Text = value;
            }
        }

        public new object Content
        {
            get
            {
                return base.Content;
            }
            private set
            {
                base.Content = value;
            }
        }

        protected override bool CanShow()
        {
            return (Icon != null || !string.IsNullOrWhiteSpace(Text));
        }

        private class NotificationContent : UserControl
        {
            private readonly BulletDecorator decorator = new BulletDecorator();
            private readonly System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            private readonly TextBlock textBlock = new TextBlock();

            private Icon icon;
            private double? iconWidth;

            public NotificationContent(Icon icon, string text)
            {
                Text = text;
                Icon = icon;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                decorator.Bullet = image;
                decorator.Margin = new Thickness(0, 4, 0, 0);
                decorator.VerticalAlignment = VerticalAlignment.Center;
            }

            public Icon Icon
            {
                get
                {
                    return icon;
                }
                set
                {
                    icon = value;

                    if (value != null)
                    {
                        image.Source = value.ToImageSource();
                        Content = decorator;

                        if (IconWidth != null)
                        {
                            image.Width = IconWidth.GetValueOrDefault();
                        }
                        else
                        {
                            image.Width = value.Width;
                        }
                    }
                    else
                    {
                        image.Source = null;
                        Content = Text;
                    }
                }
            }

            public double? IconWidth
            {
                get
                {
                    return iconWidth;
                }
                set
                {
                    iconWidth = value;

                    if (iconWidth != null)
                    {
                        image.Width = iconWidth.GetValueOrDefault();
                    }
                    else
                    {
                        image.Width = icon.Width;
                    }
                }
            }

            public string Text
            {
                get
                {
                    return textBlock.Text;
                }
                set
                {
                    textBlock.Text = value;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (value.CountLines() > 1)
                        {
                            textBlock.Margin = new Thickness(7, -3, 1, -1);
                            //image.Margin = new Thickness(0, 3, 0, 0);
                        }
                        else
                        {
                            textBlock.Margin = new Thickness(4, 2, 1, -1);
                            //image.Margin = new Thickness(0);
                        }
                        
                        decorator.Child = textBlock;
                    }
                    else
                    {
                        image.Margin = new Thickness(0);
                        decorator.Child = null;
                    }
                }
            }
        }
    }
}
