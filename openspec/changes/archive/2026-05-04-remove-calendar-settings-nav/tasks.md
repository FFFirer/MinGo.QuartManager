## 1. Remove Calendar & Settings from Sidebar

- [x] 1.1 Remove Calendar `<NavItem>` from `Sidebar.tsx` (line 80-84)
- [x] 1.2 Remove Settings `<NavItem>` from `Sidebar.tsx` (line 85-87)
- [x] 1.3 Remove unused `Calendar` and `Settings` icon imports from `Sidebar.tsx`

## 2. Remove Calendar Route from App

- [x] 2.1 Remove `import CalendarPage` from `App.tsx` (line 12)
- [x] 2.2 Remove `<Route path="/schedulers/:schedulerName/calendar">` from `App.tsx` (line 63)
- [x] 2.3 Remove Alt+C keyboard shortcut condition from `KeyboardShortcuts` (line 40)

## 3. Delete Calendar Page File

- [x] 3.1 Delete `src/MinGo.Qap.UI/src/pages/CalendarPage.tsx`

## 4. Verify

- [x] 4.1 Run LSP diagnostics on modified files (LSP未安装，目视确认)
- [x] 4.2 Confirm no dangling imports or references to Calendar/Settings
