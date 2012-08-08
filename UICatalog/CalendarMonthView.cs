//
//  CalendarMonthView.cs
//
//  Converted to MonoTouch on 1/22/09 - Eduardo Scoz || http://escoz.com
//  Originally reated by Devin Ross on 7/28/09  - tapku.com || http://github.com/devinross/tapkulibrary
//
/*
 
 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:
 
 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
 
 */
//
//  CalendarMonthView.cs
//
//  Converted to use a containing rectangle and clickable objects for iPad use on August 8, 2012 by John Ackerman ackerman.johnc@gmail.com
//  Converted to MonoTouch on 1/22/09 - Eduardo Scoz || http://escoz.com
//  Originally reated by Devin Ross on 7/28/09  - tapku.com || http://github.com/devinross/tapkulibrary
//
/*
 
 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:
 
 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
 
 */

using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace escoz
{

    public delegate void DateSelected(DateTime date);
    public delegate void MonthChanged(DateTime monthSelected);
	public delegate void EventSelected(CalendarEvent evt, RectangleF rect);

    public class CalendarMonthView : UIView
    {
		public Action<DateTime> OnDateSelected;
		public Action<DateTime> OnFinishedDateSelection;
		
        public DateTime CurrentMonthYear;
        protected DateTime CurrentDate { get; set; }

        private UIScrollView _scrollView;
        private bool calendarIsLoaded;
		
		private MonthGridView _monthGridView;
        private UIButton _leftButton, _rightButton;

		public RectangleF Container { get; set; }

		public List<CalendarEvent> Events { get; private set; }
		public event EventSelected eventSelected;

        public CalendarMonthView(RectangleF container, List<CalendarEvent> events) : base()
        {
			Container = container;
			Frame = Container;
            CurrentDate = DateTime.Now.Date;
			CurrentMonthYear = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
			Events = events;
		}
		
		public override void SetNeedsDisplay ()
		{
			base.SetNeedsDisplay ();
			if (_monthGridView != null) {
				_monthGridView.Update ();
			}
		}

		public override void LayoutSubviews ()
		{
			if (calendarIsLoaded) return;
			
			_scrollView = new UIScrollView(new RectangleF(0, 88, Container.Width, Container.Height - (Container.Height/5)))
                  {
                      ContentSize = new SizeF(Container.Width, Container.Height),
                      ScrollEnabled = false,
					  Frame = new RectangleF(0, 44, Container.Width, Container.Height-(Container.Height/5)),
                      BackgroundColor = UIColor.FromRGBA(222/255f, 222/255f, 225/255f, 1f)
                  };
            
            LoadButtons();

            LoadInitialGrids();

            BackgroundColor = UIColor.Clear;
            AddSubview(_scrollView);
			_scrollView.AddSubview(_monthGridView);
			
			calendarIsLoaded = true;
        }
		
		public void DeselectDate(){
			if (_monthGridView!=null)
				_monthGridView.DeselectDayView();
		}

        private void LoadButtons()
        {
            _leftButton = UIButton.FromType(UIButtonType.Custom);
            _leftButton.TouchUpInside += HandlePreviousMonthTouch;
            _leftButton.SetImage(UIImage.FromFile("./Images/calendar/leftarrow.png"), UIControlState.Normal);
            AddSubview(_leftButton);
            _leftButton.Frame = new RectangleF(10, 0, 44, 42);

            _rightButton = UIButton.FromType(UIButtonType.Custom);
            _rightButton.TouchUpInside += HandleNextMonthTouch;
            _rightButton.SetImage(UIImage.FromFile("./Images/calendar/rightarrow.png"), UIControlState.Normal);
            AddSubview(_rightButton);
            _rightButton.Frame = new RectangleF(Container.Width - 56, 0, 44, 42);
        }

        private void HandlePreviousMonthTouch(object sender, EventArgs e)
        {
            MoveCalendarMonths(false, true);
        }
        private void HandleNextMonthTouch(object sender, EventArgs e)
        {
            MoveCalendarMonths(true, true);
        }

        public void MoveCalendarMonths(bool upwards, bool animated)
        {
			CurrentMonthYear = CurrentMonthYear.AddMonths(upwards? 1 : -1);
			UserInteractionEnabled = false;
			
			var gridToMove = CreateNewGrid(CurrentMonthYear);
			var pointsToMove = (upwards? 0 + _monthGridView.Lines : 0 - _monthGridView.Lines) * (Container.Height/5);
			
			if (upwards && gridToMove.weekdayOfFirst==0)
				pointsToMove += (Container.Height/5);
			if (!upwards && _monthGridView.weekdayOfFirst==0)
				pointsToMove -= (Container.Height/5);
			
			gridToMove.Frame = new RectangleF(new PointF(0, pointsToMove), gridToMove.Frame.Size);
			
			_scrollView.AddSubview(gridToMove);
			
			if (animated){
				UIView.BeginAnimations("changeMonth");
				UIView.SetAnimationDuration(0.4);
				UIView.SetAnimationDelay(0.1);
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
			}
			
			_monthGridView.Center = new PointF(_monthGridView.Center.X, _monthGridView.Center.Y - pointsToMove);
			gridToMove.Center = new PointF(gridToMove.Center.X, gridToMove.Center.Y - pointsToMove);
			
			_monthGridView.Alpha = 0;
			
            _scrollView.Frame = new RectangleF(
			               _scrollView.Frame.Location,
			               new SizeF(_scrollView.Frame.Width, (gridToMove.Lines + 1) * (Container.Height/5)));
			
			_scrollView.ContentSize = _scrollView.Frame.Size;
			SetNeedsDisplay();
			
			if (animated)
				UIView.CommitAnimations();
			
			_monthGridView = gridToMove;
			_monthGridView.eventSelected += (evt, rect) => {
				eventSelected(evt, rect);
			};
			_monthGridView.Update();

            UserInteractionEnabled = true;
        }
		
		private MonthGridView CreateNewGrid(DateTime date){
			var grid = new MonthGridView(this, date);
			grid.CurrentDate = CurrentDate;
			grid.BuildGrid();
			grid.Frame = new RectangleF(0, 0, Container.Width, Container.Height);
			return grid;
		}

        private void LoadInitialGrids()
        {
            _monthGridView = CreateNewGrid(CurrentMonthYear);
			_monthGridView.eventSelected += (evt, rectangle) => {
				eventSelected(evt, rectangle);
			};
			
            var rect = _scrollView.Frame;
            rect.Size = new SizeF { Height = (_monthGridView.Lines + 1) * (Container.Height/5), Width = rect.Size.Width };
            _scrollView.Frame = rect;

            Frame = new RectangleF(Frame.X, Frame.Y, _scrollView.Frame.Size.Width, _scrollView.Frame.Size.Height+(Container.Height/5));
        }

        public override void Draw(RectangleF rect)
        {
            UIImage.FromFile("./Images/calendar/topbar.png").Draw(rect);
            DrawDayLabels(rect);
            DrawMonthLabel(rect);
        }

        private void DrawMonthLabel(RectangleF rect)
        {
            var r = new RectangleF(new PointF(0, 5), new SizeF {Width = Container.Width, Height = 42});
			UIColor.DarkGray.SetColor();
            DrawString(CurrentMonthYear.ToString("MMMM yyyy"), 
                r, UIFont.BoldSystemFontOfSize(20),
                UILineBreakMode.WordWrap, UITextAlignment.Center);
        }

        private void DrawDayLabels(RectangleF rect)
        {
            var font = UIFont.BoldSystemFontOfSize(10);
            UIColor.DarkGray.SetColor();
            var context = UIGraphics.GetCurrentContext();
            context.SaveState();
            context.SetShadowWithColor(new SizeF(0, -1), 0.5f, UIColor.White.CGColor);
            var i = 0;
            foreach (var d in Enum.GetNames(typeof(DayOfWeek)))
            {
                DrawString(d.Substring(0, 3), new RectangleF(i*((Container.Width/7) + 2), 44 - 12, Container.Width/7, 10), font,
                           UILineBreakMode.WordWrap, UITextAlignment.Center);
                i++;
            }
            context.RestoreState();
        }
    }

    public class MonthGridView : UIView
    {
		private CalendarMonthView _calendarMonthView;
		
        public DateTime CurrentDate {get;set;}
        private DateTime _currentMonth;
        protected readonly IList<CalendarDayView> _dayTiles = new List<CalendarDayView>();
        public int Lines { get; set; }
		protected CalendarDayView SelectedDayView {get;set;}
		public int weekdayOfFirst;
        public IList<DateTime> Marks { get; set; }

		public event EventSelected eventSelected;

        public MonthGridView(CalendarMonthView calendarMonthView, DateTime month)
        {
			_calendarMonthView = calendarMonthView;
            _currentMonth = month.Date;
        }
		
		public void Update(){
			foreach (var v in _dayTiles)
				updateDayView(v);
			
			this.SetNeedsDisplay();
		}

		public void updateDayView (CalendarDayView dayView)
		{
			var dayValue = int.Parse(dayView.Text);

			if (_calendarMonthView.Events.Count (ev => ev.EventDate.Date == dayView.Date.Date && ev.EventDate.Day == dayValue) > 0) {
				dayView.Events = _calendarMonthView.Events.Where(ev => ev.EventDate.Date == dayView.Date).ToList();
				dayView.eventSelected += (evt, rect) => {eventSelected(evt, rect);};
			}
		}

        public void BuildGrid()
        {
            DateTime previousMonth = _currentMonth.AddMonths(-1);
            var daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            var daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
            weekdayOfFirst = (int)_currentMonth.DayOfWeek;
            var lead = daysInPreviousMonth - (weekdayOfFirst - 1);

			float x, y;

            // build last month's days
            for (int i = 1; i <= weekdayOfFirst; i++)
            {
				var viewDay = new DateTime(_currentMonth.Year, _currentMonth.Month, i);
                var dayView = new CalendarDayView();
				x = (i - 1) * (_calendarMonthView.Container.Width/7) - 1;
				y = 0;
				dayView.Frame = new RectangleF(x, y, _calendarMonthView.Container.Width/7, _calendarMonthView.Container.Height/5);
				dayView.BackgroundColor = UIColor.White;
            	dayView.Date = viewDay;
				dayView.Text = lead.ToString();
				dayView.MyLocation = new PointF(x, y);

                AddSubview(dayView);
                _dayTiles.Add(dayView);
                lead++;
            }

            var position = weekdayOfFirst+1;
            var line = 0;

            // current month
            for (int i = 1; i <= daysInMonth; i++)
            {
				var viewDay = new DateTime(_currentMonth.Year, _currentMonth.Month, i);
				x = (position - 1) * (_calendarMonthView.Container.Width/7) - 1;
				y = line * (_calendarMonthView.Container.Height/5);
                var dayView = new CalendarDayView
                  {
                      Frame = new RectangleF(x, y, _calendarMonthView.Container.Width/7, _calendarMonthView.Container.Height/5),
                      Today = (CurrentDate.Date==viewDay.Date),
                      Text = i.ToString(),
				      BackgroundColor = UIColor.White,
                      Active = true,
                      Tag = i,
					  Selected = (i == CurrentDate.AddDays(1).Day ),
					  MyLocation = new PointF(x, y)
                  };

				dayView.Date = viewDay;
				//updateDayView(dayView);
				
				if (dayView.Selected)
					SelectedDayView = dayView;
				
                AddSubview(dayView);
                _dayTiles.Add(dayView);

                position++;
                if (position > 7)
                {
                    position = 1;
                    line++;
                }
            }

            //next month
            if (position != 1)
            {
                int dayCounter = 1;
                for (int i = position; i < 8; i++)
                {
					x = (i - 1) * ((_calendarMonthView.Container.Width/7) -1);
					y = line * (_calendarMonthView.Container.Height/5);
					var viewDay = new DateTime(_currentMonth.Year, _currentMonth.Month, i);
                    var dayView = new CalendarDayView
                      {
                          Frame = new RectangleF(x, y, _calendarMonthView.Container.Width/7, _calendarMonthView.Container.Height/5),
                          Text = dayCounter.ToString(),
						  BackgroundColor = UIColor.White,
						  MyLocation = new PointF(x, y)
                      };
					dayView.Date = viewDay;
					//updateDayView(dayView);
					
                    AddSubview(dayView);
                    _dayTiles.Add(dayView);
                    dayCounter++;
                }
            }

            Frame = new RectangleF(Frame.Location, new SizeF(Frame.Width, (line + 1) * (_calendarMonthView.Container.Height/5)));

            Lines = (position == 1 ? line - 1 : line);
			
			if (SelectedDayView!=null)
				this.BringSubviewToFront(SelectedDayView);
        }
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			if (SelectDayView((UITouch)touches.AnyObject)&& _calendarMonthView.OnDateSelected!=null)
				_calendarMonthView.OnDateSelected(new DateTime(_currentMonth.Year, _currentMonth.Month, SelectedDayView.Tag));
		}
		
		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			if (SelectDayView((UITouch)touches.AnyObject)&& _calendarMonthView.OnDateSelected!=null)
				_calendarMonthView.OnDateSelected(new DateTime(_currentMonth.Year, _currentMonth.Month, SelectedDayView.Tag));
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			if (_calendarMonthView.OnFinishedDateSelection==null) return;
			var date = new DateTime(_currentMonth.Year, _currentMonth.Month, SelectedDayView.Tag);
			_calendarMonthView.OnFinishedDateSelection(date);
		}

		private bool SelectDayView(UITouch touch){
			var p = touch.LocationInView(this);
			
			int index = ((int)p.Y / (int)(_calendarMonthView.Container.Height/5)) * 7 + ((int)p.X / (int)(_calendarMonthView.Container.Width/7));
			if(index<0 || index >= _dayTiles.Count) return false;
			
			var newSelectedDayView = _dayTiles[index];
			if (newSelectedDayView == SelectedDayView) 
				return false;
			
			if (!newSelectedDayView.Active && touch.Phase!=UITouchPhase.Moved){
				var day = int.Parse(newSelectedDayView.Text);
				if (day > 15)
					_calendarMonthView.MoveCalendarMonths(false, true);
				else
					_calendarMonthView.MoveCalendarMonths(true, true);
				return false;
			} else if (!newSelectedDayView.Active && !newSelectedDayView.Available){
				return false;
			}
			
			if (SelectedDayView!=null)
				SelectedDayView.Selected = false;
			
			this.BringSubviewToFront(newSelectedDayView);
			newSelectedDayView.Selected = true;
			
			SelectedDayView = newSelectedDayView;
			SetNeedsDisplay();
			return true;
		}
		
		public void DeselectDayView(){
			if (SelectedDayView==null) return;
			SelectedDayView.Selected= false;
			SelectedDayView = null;
			SetNeedsDisplay();
		}
    }

    public class CalendarDayView : UIView
    {
		string _text;
		public DateTime Date {get;set;}
        bool _active, _today, _selected, _marked, _available;
		public bool Available {get {return _available; } set {_available = value; SetNeedsDisplay(); }}
		public string Text {get { return _text; } set { _text = value; SetNeedsDisplay(); } }
        public bool Active {get { return _active; } set { _active = value; SetNeedsDisplay();  } }
        public bool Today {get { return _today; } set { _today = value; SetNeedsDisplay(); } }
        public bool Selected {get { return _selected; } set { _selected = value; SetNeedsDisplay(); } }
        public bool Marked {get { return _marked; } set { _marked = value; SetNeedsDisplay(); }  }
		public List<CalendarEvent> Events { get; set; }
		public PointF MyLocation {get; set;}
		public event EventSelected eventSelected;

        public override void Draw (RectangleF rect)
		{
			UIImage img;
			UIColor color;

			if (!Active || !Available) {
				color = UIColor.FromRGBA (0.576f, 0.608f, 0.647f, 1f);
				img = UIImage.FromFile ("./Images/calendar/datecell.png");
			} else if (Today && Selected) {
				color = UIColor.White;
				img = UIImage.FromFile ("./Images/calendar/todayselected.png");
			} else if (Today) {
				color = UIColor.White;
				img = UIImage.FromFile ("./Images/calendar/today.png");
			} else if (Selected) {
				color = UIColor.White;
				img = UIImage.FromFile ("./Images/calendar/datecellselected.png");
			} else if (Marked) {
				//color = UIColor.White;
				//img = UIImage.FromFile("images/calendar/datecellmarked.png");
				color = UIColor.FromRGBA (0.275f, 0.341f, 0.412f, 1f);
				img = UIImage.FromFile ("./Images/calendar/datecell.png");
			} else {
				//color = UIColor.DarkTextColor;
				color = UIColor.FromRGBA (0.275f, 0.341f, 0.412f, 1f);
				img = UIImage.FromFile ("./Images/calendar/datecell.png");
			}

			img.Draw(rect);
			//img.Draw (new PointF (0, 0));
			color.SetColor ();
			DrawString (Text, RectangleF.Inflate (Bounds, -4, 0),
                UIFont.BoldSystemFontOfSize (18), 
                UILineBreakMode.WordWrap, UITextAlignment.Right);

			if (Events != null) {
				var eventHeight = (this.Frame.Height-20)/Events.Count;
				eventHeight = eventHeight > 25 ? 25 : eventHeight;

				for(var idx = 0; idx < Events.Count; idx++) {
					//DrawString (ev.EventName + " " + ev.EventTime, RectangleF.Inflate (Bounds, 0, -40), UIFont.BoldSystemFontOfSize (10), UILineBreakMode.WordWrap, UITextAlignment.Center);

					var ev = Events[idx];
					var evBtn = UIButton.FromType(UIButtonType.Custom);
					evBtn.BackgroundColor = UIColor.FromRGB(130,182,250);
					evBtn.TitleLabel.AdjustsFontSizeToFitWidth = false;
					evBtn.TitleLabel.TextColor = UIColor.Black;
					evBtn.LineBreakMode = UILineBreakMode.Clip;
					evBtn.TitleLabel.LineBreakMode = UILineBreakMode.Clip;
					evBtn.SetTitleColor(UIColor.Black, UIControlState.Normal);
					evBtn.Layer.CornerRadius = 7.0f;
					evBtn.Layer.MasksToBounds = true;
					evBtn.Frame = new RectangleF(5, (40 - (idx * 2)) + (idx * 17), this.Frame.Width - 10, eventHeight);
					evBtn.SetTitle(ev.EventName + " " + ev.EventTime, UIControlState.Normal);
					evBtn.Font = UIFont.FromName("HelveticaNeue", 8.0f);
					evBtn.Tag = Date.Date.Day;
					evBtn.TouchUpInside += (sender, e) => {
						var btn = sender as UIButton;

						var eventObj = Events.Where(evt => evt.EventDate.Date.Day == btn.Tag && (evt.EventName + " " + evt.EventTime) == btn.Title(UIControlState.Normal)).FirstOrDefault();

						eventSelected(eventObj, new RectangleF(new PointF(MyLocation.X + btn.Frame.X, MyLocation.Y + btn.Frame.Y + 42), btn.Frame.Size));
					};
					this.AddSubview(evBtn);
				}
			}

			BackgroundColor = UIColor.White;
//            if (Marked)
//            {
//                var context = UIGraphics.GetCurrentContext();
//                if (Selected || Today)
//                    context.SetRGBFillColor(1, 1, 1, 1);
//                else if (!Active || !Available)
//					UIColor.LightGray.SetColor();
//				else
//                    context.SetRGBFillColor(75/255f, 92/255f, 111/255f, 1);
//                context.SetLineWidth(0);
//                context.AddEllipseInRect(new RectangleF(Frame.Size.Width/2 - 2, 45-10, 4, 4));
//                context.FillPath();
//
//            }
        }
    }

	public class CalendarEvent
	{
		public int Key {get; set;}
		public string EventName {get; set;}
		public string EventTime {get; set;}
		public DateTime EventDate {get; set;}
	}
}