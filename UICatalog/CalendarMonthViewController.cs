using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
namespace escoz
{
	public class CalendarMonthViewController : UIViewController
    {

        public CalendarMonthView MonthView;

        public override void ViewDidLoad()
        {
            MonthView = new CalendarMonthView(new RectangleF(0,0, 640, 1024), 
            	new List<CalendarEvent> {
            		new CalendarEvent {Key = 0, EventName = 'Test', EventTme ='2p', EventDate = new DateTime(2012, 8, 7, 14, 0, 0, 0)}
            		
            	});

	    MonthView.eventSelected += (evt, rect) => {
	    	//Do some stuff to handle the event being selected, the rectangle can be used
	    	//to present a popover.
	    }
			
            View.AddSubview(MonthView);
        }
		
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            return false;
        }

    }
}
