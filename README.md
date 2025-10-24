# Co-Parenting Calendar

A web-based calendar application designed for parents who share 50/50 custody of their child. This tool helps you organize and manage your parenting schedule throughout the year.

## Features

### Core Functionality
- **Full Year Calendar**: View and manage all 365 days of the year in a single interface
- **50/50 Split Management**: Easily assign days to either parent
- **Day Comments**: Add notes and comments to specific days
- **VAB Tracking**: Mark and track "Vård av Barn" (child care leave) days
- **Day Swapping**: Change and reassign days between parents as needed

### Additional Features
- **Custom Parent Names**: Personalize the calendar with actual parent names
- **Month Actions**: 
  - Fill entire months with one parent
  - Alternate days automatically
- **Statistics Dashboard**: View summary of day distribution, VAB days, and comments
- **Data Persistence**: All data is saved locally in your browser
- **Import/Export**: Backup and restore your calendar data

## Usage

### Getting Started
1. Open `index.html` in your web browser
2. Set the year you want to manage
3. Enter parent names in the controls section
4. Click "Update Names" to personalize the calendar

### Managing Days
- **Click any day** to open the edit dialog
- **Assign to parent**: Select which parent has the child that day
- **Mark as VAB**: Check the box to indicate child care leave
- **Add comments**: Enter any notes or special information
- **Save changes**: Click "Save" to store your updates

### Month Management
Each month has quick action buttons:
- **Fill [Parent]**: Assign all days in the month to that parent
- **Alternate Days**: Automatically alternate days between parents (odd days to Parent A, even days to Parent B)

### Data Management
- **Export Data**: Download your calendar as a JSON file for backup
- **Import Data**: Restore calendar from a previously exported file

## Color Coding

- **Blue** (Light blue background): Days assigned to Parent A
- **Pink** (Light pink background): Days assigned to Parent B
- **Gray**: Unassigned days
- **Yellow border**: VAB (child care leave) days

## Technical Details

### Technology Stack
- Pure HTML5, CSS3, and JavaScript
- No external dependencies
- Uses browser localStorage for data persistence
- Mobile-responsive design

### Data Storage
All data is stored locally in your browser using localStorage. The data includes:
- Day assignments (which parent)
- VAB markings
- Comments for each day
- Parent names

### Browser Compatibility
Works in all modern browsers that support:
- ES6 JavaScript
- localStorage API
- CSS Grid

## Privacy

This application runs entirely in your browser. No data is sent to any server. All information is stored locally on your device.

## Tips

1. **Regular Backups**: Use the Export feature regularly to backup your calendar data
2. **Year Planning**: Set up the entire year at once using the month fill features, then adjust individual days as needed
3. **Comments**: Use comments to note special occasions, holidays, or schedule exceptions
4. **VAB Tracking**: Mark VAB days to keep track of child care leave usage

## Support

For issues or questions, please open an issue on the GitHub repository.