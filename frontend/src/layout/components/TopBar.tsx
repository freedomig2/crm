import {
  Avatar,
  Button,
  Input,
  Menu,
  MenuItem,
  MenuList,
  MenuPopover,
  MenuTrigger,
  Switch,
  Tooltip,
} from '@fluentui/react-components'
import { useNavigate } from 'react-router-dom'
import {
  AlertRegular,
  PanelLeftRegular,
  SearchRegular,
  SettingsRegular,
} from '@fluentui/react-icons'
import { useAuth } from '../../auth/AuthContext'
import styles from './TopBar.module.css'

export function TopBar({
  collapsed,
  onToggleSidebar,
  darkMode,
  onToggleDarkMode,
  userEmail,
}: {
  collapsed: boolean
  onToggleSidebar: () => void
  darkMode: boolean
  onToggleDarkMode: (value: boolean) => void
  userEmail?: string
}) {
  const { logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = async () => {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className={styles.bar}>
      <div className={styles.left}>
        <Tooltip content={collapsed ? 'Expand menu' : 'Collapse menu'} relationship="label">
          <Button data-testid="toggle-sidebar" icon={<PanelLeftRegular />} appearance="subtle" size="small" onClick={onToggleSidebar} />
        </Tooltip>
      </div>

      <div className={styles.searchWrap}>
        <Input
          size="small"
          contentBefore={<SearchRegular />}
          placeholder="Search users, roles, settings..."
          className={styles.searchInput}
        />
      </div>

      <div className={styles.right}>
        <Tooltip content="Notifications" relationship="label">
          <Button icon={<AlertRegular />} appearance="subtle" size="small" />
        </Tooltip>
        <Tooltip content="Settings" relationship="label">
          <Button icon={<SettingsRegular />} appearance="subtle" size="small" />
        </Tooltip>

        <Tooltip content="Theme" relationship="label">
          <Switch checked={darkMode} onChange={(_, data) => onToggleDarkMode(data.checked)} />
        </Tooltip>

        <Menu>
          <MenuTrigger disableButtonEnhancement>
            <Button appearance="subtle" size="small" icon={<Avatar name={userEmail ?? 'User'} size={24} />} />
          </MenuTrigger>
          <MenuPopover>
            <MenuList>
              <MenuItem>{userEmail ?? 'Unknown user'}</MenuItem>
              <MenuItem onClick={() => navigate('/security/change-password')}>Change Password</MenuItem>
              <MenuItem onClick={handleLogout}>Logout</MenuItem>
            </MenuList>
          </MenuPopover>
        </Menu>
      </div>
    </div>
  )
}
