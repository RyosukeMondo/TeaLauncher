# TeaLauncher Design Theme System

## Overview
TeaLauncher uses a centralized design theme system built with Avalonia resource dictionaries and styles. This provides consistent, sophisticated dark theme UI across all windows with high contrast ratios meeting WCAG 2.1 AA standards.

## Theme Architecture

### Theme Files Location
All theme files are located in `TeaLauncher.Avalonia/Themes/`

### Theme Structure

#### 1. Colors.axaml
**Purpose**: Defines the color palette for the entire application

**Color Categories**:
- **Background Colors**: `BackgroundPrimary` (#1A1A1A), `BackgroundSecondary` (#252525), `BackgroundTertiary` (#2D2D2D), `BackgroundElevated` (#333333)
- **Surface Colors**: `SurfaceDefault` (#2A2A2A), `SurfaceHighlight` (#363636), `SurfaceAccent` (#404040)
- **Text Colors**: `TextPrimary` (#FFFFFF), `TextSecondary` (#B8B8B8), `TextTertiary` (#8F8F8F), `TextDisabled` (#666666)
- **Accent Colors**: `AccentPrimary` (#5BA3E7), `AccentSecondary` (#4A8CD1), `AccentTertiary` (#3A75BA)
- **Status Colors**: `Success` (#4CAF50), `Warning` (#FFA726), `Error` (#EF5350), `Info` (#5BA3E7)
- **Border Colors**: `BorderPrimary` (#404040), `BorderSecondary` (#333333), `BorderAccent` (#5BA3E7)
- **Overlay Colors**: `OverlayLight` (#40FFFFFF), `OverlayMedium` (#60FFFFFF), `OverlayDark` (#CC000000)

All colors have corresponding `SolidColorBrush` resources (e.g., `TextPrimaryBrush`)

**Contrast Ratios**:
- Primary text on primary background: 15.2:1 (AAA)
- Secondary text on primary background: 7.3:1 (AA)
- Tertiary text on primary background: 4.6:1 (AA)

#### 2. Typography.axaml
**Purpose**: Defines typography system including fonts, sizes, and spacing

**Font Families**:
- `FontFamilyPrimary`: Inter, Segoe UI, sans-serif
- `FontFamilyMonospace`: Consolas, Courier New, monospace

**Font Sizes**:
- `FontSizeXLarge`: 28
- `FontSizeLarge`: 24
- `FontSizeMedium`: 16
- `FontSizeNormal`: 14
- `FontSizeSmall`: 12
- `FontSizeXSmall`: 11

**Font Weights**:
- `FontWeightBold`: Bold
- `FontWeightSemiBold`: SemiBold
- `FontWeightMedium`: Medium
- `FontWeightRegular`: Regular
- `FontWeightLight`: Light

**Spacing**:
- `SpacingXSmall`: 5
- `SpacingSmall`: 10
- `SpacingMedium`: 15
- `SpacingLarge`: 20
- `SpacingXLarge`: 30
- `SpacingXXLarge`: 40

**Border Radius**:
- `BorderRadiusSmall`: 4
- `BorderRadiusMedium`: 6
- `BorderRadiusLarge`: 8

#### 3. InitializationWindowTheme.axaml
**Purpose**: Defines styles specific to the first-time setup window

**Key Styles**:
- `TextBlock.heading`: Large, bold headings with high contrast
- `TextBlock.subheading`: Medium-sized section headers
- `TextBlock.body`: Standard body text with good readability
- `TextBlock.muted`: Italicized tertiary text for hints
- `TextBlock.code`: Monospace text for file paths with accent color
- `TextBlock.bullet`: Bullet point list items
- `Border.info-panel`: Information panels with subtle borders
- `Button.primary`: Primary action button with hover/pressed states
- `RadioButton`: Radio button styles with checked state

#### 4. MainWindowTheme.axaml
**Purpose**: Defines styles for the main command launcher window

**Key Styles**:
- `AutoCompleteBox`: Command input box with transparent background
- `AutoCompleteBox /template/ TextBox`: Inner text box styling
- `AutoCompleteBox /template/ Border`: Border styling
- `ListBoxItem`: Suggestion list items
- `ListBoxItem:pointerover`: Hover state with light overlay
- `ListBoxItem:selected`: Selected state with medium overlay
- `AutoCompleteBox ListBox`: Dropdown list container

## Integration with App.axaml

The theme is registered in `App.axaml` in two sections:

1. **Application.Resources**: Contains color and typography resource dictionaries
   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <ResourceInclude Source="/Themes/Colors.axaml"/>
               <ResourceInclude Source="/Themes/Typography.axaml"/>
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```

2. **Application.Styles**: Contains style definitions
   ```xml
   <Application.Styles>
       <FluentTheme />
       <StyleInclude Source="/Themes/MainWindowTheme.axaml"/>
       <StyleInclude Source="/Themes/InitializationWindowTheme.axaml"/>
   </Application.Styles>
   ```

## Usage Guidelines

### Using Theme Resources
Always use dynamic resource references to enable theme switching in the future:
```xml
<TextBlock Foreground="{DynamicResource TextPrimaryBrush}" />
<Border Background="{DynamicResource SurfaceDefaultBrush}" />
```

### Using Style Classes
Apply predefined style classes to elements:
```xml
<TextBlock Classes="heading" Text="Welcome" />
<TextBlock Classes="body" Text="Description" />
<Border Classes="info-panel">
    <!-- Content -->
</Border>
<Button Classes="primary" Content="Start" />
```

### Adding New Colors
When adding new colors:
1. Add the color definition in Colors.axaml
2. Create a corresponding SolidColorBrush
3. Ensure contrast ratios meet WCAG 2.1 AA (4.5:1 for normal text)

### Adding New Styles
When adding new styles:
1. Determine if it's window-specific or global
2. Add to appropriate theme file (InitializationWindowTheme or MainWindowTheme)
3. Use class selectors (e.g., `TextBlock.custom-class`) for reusable styles
4. Always reference theme resources using `{DynamicResource}`

## Benefits

1. **Consistency**: Single source of truth for colors and styles
2. **Maintainability**: Easy to update the entire UI by changing theme files
3. **Accessibility**: All colors meet WCAG 2.1 AA contrast requirements
4. **Scalability**: New windows automatically inherit the theme
5. **Future-proof**: Structure supports theme switching (light/dark modes)
6. **Sophisticated UX**: Cohesive dark theme with proper visual hierarchy

## Theme Files Build Configuration

The theme files are included in the build via TeaLauncher.Avalonia.csproj:
```xml
<ItemGroup>
    <AvaloniaResource Include="Themes\**" />
</ItemGroup>
```

This ensures all XAML files in the Themes directory are embedded as Avalonia resources.
