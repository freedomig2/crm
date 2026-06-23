import { Navigate, Route, Routes, useParams } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { AppShell } from './layout/components/AppShell'
import { ForgotPasswordPage } from './pages/ForgotPasswordPage'
import { ChangePasswordPage } from './pages/ChangePasswordPage'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './dashboard/DashboardPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { ResetPasswordPage } from './pages/ResetPasswordPage'
import { SimpleStatePage } from './pages/SimpleStatePage'
import { useState } from 'react'
import { FluentProvider, webDarkTheme, webLightTheme } from '@fluentui/react-components'
import './index.css'
import {
  accountActivitiesConfig,
  accountAddressesConfig,
  accountRelationshipsConfig,
  accountsConfig,
  auditLogsConfig,
  customerProfilesConfig,
  discountsConfig,
  departmentsConfig,
  lookupCategoriesConfig,
  lookupValuesConfig,
  permissionsConfig,
  priceListsConfig,
  productBundlesConfig,
  productCategoriesConfig,
  productsConfig,
  quotesConfig,
  rolesConfig,
  settingsConfig,
  teamsConfig,
  unitOfMeasuresConfig,
  usersConfig,
} from './components/crud/adminConfig'
import { EntityListPage } from './components/crud/EntityListPage'
import { EntityCreatePage } from './components/crud/EntityCreatePage'
import { EntityEditPage } from './components/crud/EntityEditPage'
import { EntityDetailsPage } from './components/crud/EntityDetailsPage'
import { ContactsListPage } from './contacts/ContactsListPage'
import { ContactFormPage } from './contacts/ContactFormPage'
import { ContactDetailsPage } from './contacts/ContactDetailsPage'
import { LeadsListPage } from './leads/LeadsListPage'
import { LeadFormPage } from './leads/LeadFormPage'
import { LeadDetailsPage } from './leads/LeadDetailsPage'
import { LeadConversionPage } from './leads/LeadConversionPage'
import { LeadTimelinePage } from './leads/LeadTimelinePage'
import { LeadScoreRuleDetailsPage, LeadScoreRuleFormPage, LeadScoreRulesListPage } from './leads/LeadScoreRulesPages'
import { NumberSequenceDetailsPage, NumberSequenceFormPage, NumberSequencesListPage } from './configuration/NumberSequencesPages'
import { OpportunitiesListPage } from './opportunities/OpportunitiesListPage'
import { OpportunityFormPage } from './opportunities/OpportunityFormPage'
import { OpportunityDetailsPage } from './opportunities/OpportunityDetailsPage'
import { OpportunityTimelinePage } from './opportunities/OpportunityTimelinePage'
import { OpportunityOutcomePage } from './opportunities/OpportunityOutcomePage'
import { SalesPipelinePage } from './sales/SalesPipelinePage'
import { ForecastDetailsPage, ForecastFormPage, ForecastsPage } from './sales/ForecastsPages'
import { SalesTargetDetailsPage, SalesTargetFormPage, SalesTargetsListPage } from './sales/SalesTargetsPages'
import { RevenueTrackingPage, SalesPerformancePage } from './sales/SalesAnalyticsPages'
import { PriceListItemsPage, ProductBundleItemsPage } from './sales/ProductPricingPages'
import { QuoteLinesPage } from './sales/QuoteLinesPage'

function LegacyContactRedirect({ edit }: { edit?: boolean }) {
  const { id } = useParams()
  if (!id) {
    return <Navigate to="/contacts" replace />
  }

  return <Navigate to={`/contacts/${id}${edit ? '/edit' : ''}`} replace />
}

function LegacyLeadRedirect({ edit, child }: { edit?: boolean; child?: 'convert' | 'timeline' }) {
  const { id } = useParams()
  if (!id) {
    return <Navigate to="/leads" replace />
  }

  const suffix = child ? `/${child}` : edit ? '/edit' : ''
  return <Navigate to={`/leads/${id}${suffix}`} replace />
}

function LegacyOpportunityRedirect({ edit, child }: { edit?: boolean; child?: 'timeline' | 'mark-won' | 'mark-lost' }) {
  const { id } = useParams()
  if (!id) {
    return <Navigate to="/opportunities" replace />
  }

  const suffix = child ? `/${child}` : edit ? '/edit' : ''
  return <Navigate to={`/opportunities/${id}${suffix}`} replace />
}

function App() {
  const [darkMode, setDarkMode] = useState(false)

  return (
    <FluentProvider theme={darkMode ? webDarkTheme : webLightTheme}>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />

          <Route element={<ProtectedRoute />}>
            <Route element={<AppShell darkMode={darkMode} onToggleDarkMode={setDarkMode} />}>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/admin/users" element={<EntityListPage config={usersConfig} title={usersConfig.title} subtitle={usersConfig.subtitle} endpoint={usersConfig.endpoint} columns={usersConfig.columns} listPath={usersConfig.listPath} createPath={usersConfig.createPath} detailsPath={usersConfig.detailsPath} editPath={usersConfig.editPath} permissions={usersConfig.permissions} />} />
              <Route path="/admin/users/create" element={<EntityCreatePage config={usersConfig} />} />
              <Route path="/admin/users/:id/edit" element={<EntityEditPage config={usersConfig} />} />
              <Route path="/admin/users/:id" element={<EntityDetailsPage config={usersConfig} />} />

              <Route path="/admin/roles" element={<EntityListPage config={rolesConfig} title={rolesConfig.title} subtitle={rolesConfig.subtitle} endpoint={rolesConfig.endpoint} columns={rolesConfig.columns} listPath={rolesConfig.listPath} createPath={rolesConfig.createPath} detailsPath={rolesConfig.detailsPath} editPath={rolesConfig.editPath} permissions={rolesConfig.permissions} />} />
              <Route path="/admin/roles/create" element={<EntityCreatePage config={rolesConfig} />} />
              <Route path="/admin/roles/:id/edit" element={<EntityEditPage config={rolesConfig} />} />
              <Route path="/admin/roles/:id" element={<EntityDetailsPage config={rolesConfig} />} />

              <Route path="/admin/permissions" element={<EntityListPage config={permissionsConfig} title={permissionsConfig.title} subtitle={permissionsConfig.subtitle} endpoint={permissionsConfig.endpoint} columns={permissionsConfig.columns} listPath={permissionsConfig.listPath} createPath={permissionsConfig.createPath} detailsPath={permissionsConfig.detailsPath} editPath={permissionsConfig.editPath} permissions={permissionsConfig.permissions} />} />
              <Route path="/admin/permissions/create" element={<EntityCreatePage config={permissionsConfig} />} />
              <Route path="/admin/permissions/:id/edit" element={<EntityEditPage config={permissionsConfig} />} />
              <Route path="/admin/permissions/:id" element={<EntityDetailsPage config={permissionsConfig} />} />

              <Route path="/admin/teams" element={<EntityListPage config={teamsConfig} title={teamsConfig.title} subtitle={teamsConfig.subtitle} endpoint={teamsConfig.endpoint} columns={teamsConfig.columns} listPath={teamsConfig.listPath} createPath={teamsConfig.createPath} detailsPath={teamsConfig.detailsPath} editPath={teamsConfig.editPath} permissions={teamsConfig.permissions} />} />
              <Route path="/admin/teams/create" element={<EntityCreatePage config={teamsConfig} />} />
              <Route path="/admin/teams/:id/edit" element={<EntityEditPage config={teamsConfig} />} />
              <Route path="/admin/teams/:id" element={<EntityDetailsPage config={teamsConfig} />} />

              <Route path="/admin/departments" element={<EntityListPage config={departmentsConfig} title={departmentsConfig.title} subtitle={departmentsConfig.subtitle} endpoint={departmentsConfig.endpoint} columns={departmentsConfig.columns} listPath={departmentsConfig.listPath} createPath={departmentsConfig.createPath} detailsPath={departmentsConfig.detailsPath} editPath={departmentsConfig.editPath} permissions={departmentsConfig.permissions} />} />
              <Route path="/admin/departments/create" element={<EntityCreatePage config={departmentsConfig} />} />
              <Route path="/admin/departments/:id/edit" element={<EntityEditPage config={departmentsConfig} />} />
              <Route path="/admin/departments/:id" element={<EntityDetailsPage config={departmentsConfig} />} />

              <Route path="/admin/system-settings" element={<EntityListPage config={settingsConfig} title={settingsConfig.title} subtitle={settingsConfig.subtitle} endpoint={settingsConfig.endpoint} columns={settingsConfig.columns} listPath={settingsConfig.listPath} createPath={settingsConfig.createPath} detailsPath={settingsConfig.detailsPath} editPath={settingsConfig.editPath} permissions={settingsConfig.permissions} />} />
              <Route path="/admin/system-settings/create" element={<EntityCreatePage config={settingsConfig} />} />
              <Route path="/admin/system-settings/:id/edit" element={<EntityEditPage config={settingsConfig} />} />
              <Route path="/admin/system-settings/:id" element={<EntityDetailsPage config={settingsConfig} />} />

              <Route path="/admin/lookup-categories" element={<EntityListPage config={lookupCategoriesConfig} title={lookupCategoriesConfig.title} subtitle={lookupCategoriesConfig.subtitle} endpoint={lookupCategoriesConfig.endpoint} columns={lookupCategoriesConfig.columns} listPath={lookupCategoriesConfig.listPath} createPath={lookupCategoriesConfig.createPath} detailsPath={lookupCategoriesConfig.detailsPath} editPath={lookupCategoriesConfig.editPath} permissions={lookupCategoriesConfig.permissions} />} />
              <Route path="/admin/lookup-categories/create" element={<EntityCreatePage config={lookupCategoriesConfig} />} />
              <Route path="/admin/lookup-categories/:id/edit" element={<EntityEditPage config={lookupCategoriesConfig} />} />
              <Route path="/admin/lookup-categories/:id" element={<EntityDetailsPage config={lookupCategoriesConfig} />} />

              <Route path="/admin/lookup-values" element={<EntityListPage config={lookupValuesConfig} title={lookupValuesConfig.title} subtitle={lookupValuesConfig.subtitle} endpoint={lookupValuesConfig.endpoint} columns={lookupValuesConfig.columns} listPath={lookupValuesConfig.listPath} createPath={lookupValuesConfig.createPath} detailsPath={lookupValuesConfig.detailsPath} editPath={lookupValuesConfig.editPath} permissions={lookupValuesConfig.permissions} />} />
              <Route path="/admin/lookup-values/create" element={<EntityCreatePage config={lookupValuesConfig} />} />
              <Route path="/admin/lookup-values/:id/edit" element={<EntityEditPage config={lookupValuesConfig} />} />
              <Route path="/admin/lookup-values/:id" element={<EntityDetailsPage config={lookupValuesConfig} />} />

              <Route path="/admin/audit-logs" element={<EntityListPage config={auditLogsConfig} title={auditLogsConfig.title} subtitle={auditLogsConfig.subtitle} endpoint={auditLogsConfig.endpoint} columns={auditLogsConfig.columns} listPath={auditLogsConfig.listPath} createPath={auditLogsConfig.createPath} detailsPath={auditLogsConfig.detailsPath} editPath={auditLogsConfig.editPath} permissions={auditLogsConfig.permissions} />} />
              <Route path="/admin/audit-logs/:id" element={<EntityDetailsPage config={auditLogsConfig} />} />

              <Route path="/crm/accounts" element={<EntityListPage config={accountsConfig} title={accountsConfig.title} subtitle={accountsConfig.subtitle} endpoint={accountsConfig.endpoint} columns={accountsConfig.columns} listPath={accountsConfig.listPath} createPath={accountsConfig.createPath} detailsPath={accountsConfig.detailsPath} editPath={accountsConfig.editPath} permissions={accountsConfig.permissions} />} />
              <Route path="/crm/accounts/create" element={<EntityCreatePage config={accountsConfig} />} />
              <Route path="/crm/accounts/:id/edit" element={<EntityEditPage config={accountsConfig} />} />
              <Route path="/crm/accounts/:id" element={<EntityDetailsPage config={accountsConfig} />} />

              <Route path="/contacts" element={<ContactsListPage />} />
              <Route path="/contacts/create" element={<ContactFormPage mode="create" />} />
              <Route path="/contacts/:id/edit" element={<ContactFormPage mode="edit" />} />
              <Route path="/contacts/:id" element={<ContactDetailsPage />} />
              <Route path="/crm/contacts" element={<Navigate to="/contacts" replace />} />
              <Route path="/crm/contacts/create" element={<Navigate to="/contacts/create" replace />} />
              <Route path="/crm/contacts/:id/edit" element={<LegacyContactRedirect edit />} />
              <Route path="/crm/contacts/:id" element={<LegacyContactRedirect />} />

              <Route path="/leads" element={<LeadsListPage />} />
              <Route path="/leads/create" element={<LeadFormPage mode="create" />} />
              <Route path="/leads/:id/edit" element={<LeadFormPage mode="edit" />} />
              <Route path="/leads/:id/convert" element={<LeadConversionPage />} />
              <Route path="/leads/:id/timeline" element={<LeadTimelinePage />} />
              <Route path="/leads/:id" element={<LeadDetailsPage />} />
              <Route path="/sales/leads" element={<Navigate to="/leads" replace />} />
              <Route path="/sales/leads/create" element={<Navigate to="/leads/create" replace />} />
              <Route path="/sales/leads/:id/edit" element={<LegacyLeadRedirect edit />} />
              <Route path="/sales/leads/:id/convert" element={<LegacyLeadRedirect child="convert" />} />
              <Route path="/sales/leads/:id/timeline" element={<LegacyLeadRedirect child="timeline" />} />
              <Route path="/sales/leads/:id" element={<LegacyLeadRedirect />} />

              <Route path="/lead-score-rules" element={<LeadScoreRulesListPage />} />
              <Route path="/lead-score-rules/create" element={<LeadScoreRuleFormPage mode="create" />} />
              <Route path="/lead-score-rules/:id/edit" element={<LeadScoreRuleFormPage mode="edit" />} />
              <Route path="/lead-score-rules/:id" element={<LeadScoreRuleDetailsPage />} />

              <Route path="/opportunities" element={<OpportunitiesListPage />} />
              <Route path="/opportunities/create" element={<OpportunityFormPage mode="create" />} />
              <Route path="/opportunities/pipeline" element={<Navigate to="/sales/pipeline" replace />} />
              <Route path="/opportunities/:id/edit" element={<OpportunityFormPage mode="edit" />} />
              <Route path="/opportunities/:id/timeline" element={<OpportunityTimelinePage />} />
              <Route path="/opportunities/:id/mark-won" element={<OpportunityOutcomePage mode="won" />} />
              <Route path="/opportunities/:id/mark-lost" element={<OpportunityOutcomePage mode="lost" />} />
              <Route path="/opportunities/:id" element={<OpportunityDetailsPage />} />
              <Route path="/sales/opportunities" element={<Navigate to="/opportunities" replace />} />
              <Route path="/sales/opportunities/create" element={<Navigate to="/opportunities/create" replace />} />
              <Route path="/sales/opportunities/pipeline" element={<Navigate to="/sales/pipeline" replace />} />
              <Route path="/sales/opportunities/:id/edit" element={<LegacyOpportunityRedirect edit />} />
              <Route path="/sales/opportunities/:id/timeline" element={<LegacyOpportunityRedirect child="timeline" />} />
              <Route path="/sales/opportunities/:id/mark-won" element={<LegacyOpportunityRedirect child="mark-won" />} />
              <Route path="/sales/opportunities/:id/mark-lost" element={<LegacyOpportunityRedirect child="mark-lost" />} />
              <Route path="/sales/opportunities/:id" element={<LegacyOpportunityRedirect />} />

              <Route path="/sales/pipeline" element={<SalesPipelinePage />} />
              <Route path="/sales/forecasts" element={<ForecastsPage />} />
              <Route path="/sales/forecasts/create" element={<ForecastFormPage mode="create" />} />
              <Route path="/sales/forecasts/:id/edit" element={<ForecastFormPage mode="edit" />} />
              <Route path="/sales/forecasts/:id" element={<ForecastDetailsPage />} />
              <Route path="/sales/targets" element={<SalesTargetsListPage />} />
              <Route path="/sales/targets/create" element={<SalesTargetFormPage mode="create" />} />
              <Route path="/sales/targets/:id/edit" element={<SalesTargetFormPage mode="edit" />} />
              <Route path="/sales/targets/:id" element={<SalesTargetDetailsPage />} />
              <Route path="/sales/revenue" element={<RevenueTrackingPage />} />
              <Route path="/sales/performance" element={<SalesPerformancePage />} />

              <Route path="/sales/products" element={<EntityListPage config={productsConfig} title={productsConfig.title} subtitle={productsConfig.subtitle} endpoint={productsConfig.endpoint} columns={productsConfig.columns} listPath={productsConfig.listPath} createPath={productsConfig.createPath} detailsPath={productsConfig.detailsPath} editPath={productsConfig.editPath} permissions={productsConfig.permissions} />} />
              <Route path="/sales/products/create" element={<EntityCreatePage config={productsConfig} />} />
              <Route path="/sales/products/:id/edit" element={<EntityEditPage config={productsConfig} />} />
              <Route path="/sales/products/:id" element={<EntityDetailsPage config={productsConfig} />} />

              <Route path="/sales/product-categories" element={<EntityListPage config={productCategoriesConfig} title={productCategoriesConfig.title} subtitle={productCategoriesConfig.subtitle} endpoint={productCategoriesConfig.endpoint} columns={productCategoriesConfig.columns} listPath={productCategoriesConfig.listPath} createPath={productCategoriesConfig.createPath} detailsPath={productCategoriesConfig.detailsPath} editPath={productCategoriesConfig.editPath} permissions={productCategoriesConfig.permissions} />} />
              <Route path="/sales/product-categories/create" element={<EntityCreatePage config={productCategoriesConfig} />} />
              <Route path="/sales/product-categories/:id/edit" element={<EntityEditPage config={productCategoriesConfig} />} />
              <Route path="/sales/product-categories/:id" element={<EntityDetailsPage config={productCategoriesConfig} />} />

              <Route path="/sales/price-lists" element={<EntityListPage config={priceListsConfig} title={priceListsConfig.title} subtitle={priceListsConfig.subtitle} endpoint={priceListsConfig.endpoint} columns={priceListsConfig.columns} listPath={priceListsConfig.listPath} createPath={priceListsConfig.createPath} detailsPath={priceListsConfig.detailsPath} editPath={priceListsConfig.editPath} permissions={priceListsConfig.permissions} />} />
              <Route path="/sales/price-lists/create" element={<EntityCreatePage config={priceListsConfig} />} />
              <Route path="/sales/price-lists/:id/edit" element={<EntityEditPage config={priceListsConfig} />} />
              <Route path="/sales/price-lists/:id" element={<EntityDetailsPage config={priceListsConfig} />} />
              <Route path="/sales/price-lists/:id/items" element={<PriceListItemsPage />} />

              <Route path="/sales/product-bundles" element={<EntityListPage config={productBundlesConfig} title={productBundlesConfig.title} subtitle={productBundlesConfig.subtitle} endpoint={productBundlesConfig.endpoint} columns={productBundlesConfig.columns} listPath={productBundlesConfig.listPath} createPath={productBundlesConfig.createPath} detailsPath={productBundlesConfig.detailsPath} editPath={productBundlesConfig.editPath} permissions={productBundlesConfig.permissions} />} />
              <Route path="/sales/product-bundles/create" element={<EntityCreatePage config={productBundlesConfig} />} />
              <Route path="/sales/product-bundles/:id/edit" element={<EntityEditPage config={productBundlesConfig} />} />
              <Route path="/sales/product-bundles/:id" element={<EntityDetailsPage config={productBundlesConfig} />} />
              <Route path="/sales/product-bundles/:id/items" element={<ProductBundleItemsPage />} />

              <Route path="/sales/unit-of-measures" element={<EntityListPage config={unitOfMeasuresConfig} title={unitOfMeasuresConfig.title} subtitle={unitOfMeasuresConfig.subtitle} endpoint={unitOfMeasuresConfig.endpoint} columns={unitOfMeasuresConfig.columns} listPath={unitOfMeasuresConfig.listPath} createPath={unitOfMeasuresConfig.createPath} detailsPath={unitOfMeasuresConfig.detailsPath} editPath={unitOfMeasuresConfig.editPath} permissions={unitOfMeasuresConfig.permissions} />} />
              <Route path="/sales/unit-of-measures/create" element={<EntityCreatePage config={unitOfMeasuresConfig} />} />
              <Route path="/sales/unit-of-measures/:id/edit" element={<EntityEditPage config={unitOfMeasuresConfig} />} />
              <Route path="/sales/unit-of-measures/:id" element={<EntityDetailsPage config={unitOfMeasuresConfig} />} />

              <Route path="/sales/discounts" element={<EntityListPage config={discountsConfig} title={discountsConfig.title} subtitle={discountsConfig.subtitle} endpoint={discountsConfig.endpoint} columns={discountsConfig.columns} listPath={discountsConfig.listPath} createPath={discountsConfig.createPath} detailsPath={discountsConfig.detailsPath} editPath={discountsConfig.editPath} permissions={discountsConfig.permissions} />} />
              <Route path="/sales/discounts/create" element={<EntityCreatePage config={discountsConfig} />} />
              <Route path="/sales/discounts/:id/edit" element={<EntityEditPage config={discountsConfig} />} />
              <Route path="/sales/discounts/:id" element={<EntityDetailsPage config={discountsConfig} />} />

              <Route path="/sales/quotes" element={<EntityListPage config={quotesConfig} title={quotesConfig.title} subtitle={quotesConfig.subtitle} endpoint={quotesConfig.endpoint} columns={quotesConfig.columns} listPath={quotesConfig.listPath} createPath={quotesConfig.createPath} detailsPath={quotesConfig.detailsPath} editPath={quotesConfig.editPath} permissions={quotesConfig.permissions} />} />
              <Route path="/sales/quotes/create" element={<EntityCreatePage config={quotesConfig} />} />
              <Route path="/sales/quotes/:id/edit" element={<EntityEditPage config={quotesConfig} />} />
              <Route path="/sales/quotes/:id" element={<EntityDetailsPage config={quotesConfig} />} />
              <Route path="/sales/quotes/:id/lines" element={<QuoteLinesPage />} />

              <Route path="/crm/account-addresses" element={<EntityListPage config={accountAddressesConfig} title={accountAddressesConfig.title} subtitle={accountAddressesConfig.subtitle} endpoint={accountAddressesConfig.endpoint} columns={accountAddressesConfig.columns} listPath={accountAddressesConfig.listPath} createPath={accountAddressesConfig.createPath} detailsPath={accountAddressesConfig.detailsPath} editPath={accountAddressesConfig.editPath} permissions={accountAddressesConfig.permissions} />} />
              <Route path="/crm/account-addresses/create" element={<EntityCreatePage config={accountAddressesConfig} />} />
              <Route path="/crm/account-addresses/:id/edit" element={<EntityEditPage config={accountAddressesConfig} />} />
              <Route path="/crm/account-addresses/:id" element={<EntityDetailsPage config={accountAddressesConfig} />} />

              <Route path="/crm/customer-profiles" element={<EntityListPage config={customerProfilesConfig} title={customerProfilesConfig.title} subtitle={customerProfilesConfig.subtitle} endpoint={customerProfilesConfig.endpoint} columns={customerProfilesConfig.columns} listPath={customerProfilesConfig.listPath} createPath={customerProfilesConfig.createPath} detailsPath={customerProfilesConfig.detailsPath} editPath={customerProfilesConfig.editPath} permissions={customerProfilesConfig.permissions} />} />
              <Route path="/crm/customer-profiles/create" element={<EntityCreatePage config={customerProfilesConfig} />} />
              <Route path="/crm/customer-profiles/:id/edit" element={<EntityEditPage config={customerProfilesConfig} />} />
              <Route path="/crm/customer-profiles/:id" element={<EntityDetailsPage config={customerProfilesConfig} />} />

              <Route path="/crm/account-relationships" element={<EntityListPage config={accountRelationshipsConfig} title={accountRelationshipsConfig.title} subtitle={accountRelationshipsConfig.subtitle} endpoint={accountRelationshipsConfig.endpoint} columns={accountRelationshipsConfig.columns} listPath={accountRelationshipsConfig.listPath} createPath={accountRelationshipsConfig.createPath} detailsPath={accountRelationshipsConfig.detailsPath} editPath={accountRelationshipsConfig.editPath} permissions={accountRelationshipsConfig.permissions} />} />
              <Route path="/crm/account-relationships/create" element={<EntityCreatePage config={accountRelationshipsConfig} />} />
              <Route path="/crm/account-relationships/:id/edit" element={<EntityEditPage config={accountRelationshipsConfig} />} />
              <Route path="/crm/account-relationships/:id" element={<EntityDetailsPage config={accountRelationshipsConfig} />} />

              <Route path="/crm/account-activities" element={<EntityListPage config={accountActivitiesConfig} title={accountActivitiesConfig.title} subtitle={accountActivitiesConfig.subtitle} endpoint={accountActivitiesConfig.endpoint} columns={accountActivitiesConfig.columns} listPath={accountActivitiesConfig.listPath} createPath={accountActivitiesConfig.createPath} detailsPath={accountActivitiesConfig.detailsPath} editPath={accountActivitiesConfig.editPath} permissions={accountActivitiesConfig.permissions} />} />
              <Route path="/crm/account-activities/create" element={<EntityCreatePage config={accountActivitiesConfig} />} />
              <Route path="/crm/account-activities/:id/edit" element={<EntityEditPage config={accountActivitiesConfig} />} />
              <Route path="/crm/account-activities/:id" element={<EntityDetailsPage config={accountActivitiesConfig} />} />

              <Route path="/users" element={<Navigate to="/admin/users" replace />} />
              <Route path="/roles" element={<Navigate to="/admin/roles" replace />} />
              <Route path="/permissions" element={<Navigate to="/admin/permissions" replace />} />
              <Route path="/teams" element={<Navigate to="/admin/teams" replace />} />
              <Route path="/departments" element={<Navigate to="/admin/departments" replace />} />
              <Route path="/system-settings" element={<Navigate to="/admin/system-settings" replace />} />
              <Route path="/configuration/lookup-categories" element={<Navigate to="/admin/lookup-categories" replace />} />
              <Route path="/configuration/lookup-values" element={<Navigate to="/admin/lookup-values" replace />} />
              <Route path="/audit-logs" element={<Navigate to="/admin/audit-logs" replace />} />
              <Route path="/accounts" element={<Navigate to="/crm/accounts" replace />} />

              <Route path="/security/login-history" element={<SimpleStatePage title="Login History" subtitle="Review login timeline, devices, and geolocation details." />} />
              <Route path="/security/active-sessions" element={<SimpleStatePage title="Active Sessions" subtitle="Monitor active sessions and terminate risky sessions quickly." />} />
              <Route path="/security/failed-logins" element={<SimpleStatePage title="Failed Login Attempts" subtitle="Analyze failed sign-in patterns and suspicious accounts." />} />
              <Route path="/security/change-password" element={<ChangePasswordPage />} />
              <Route path="/security/password-policies" element={<SimpleStatePage title="Password Policies" subtitle="Configure enterprise-grade password complexity and expiry rules." />} />
              <Route path="/security/mfa-settings" element={<SimpleStatePage title="MFA Settings" subtitle="Enforce multi-factor authentication and authentication methods." />} />

              <Route path="/configuration/number-sequences" element={<NumberSequencesListPage />} />
              <Route path="/configuration/number-sequences/create" element={<NumberSequenceFormPage mode="create" />} />
              <Route path="/configuration/number-sequences/:id/edit" element={<NumberSequenceFormPage mode="edit" />} />
              <Route path="/configuration/number-sequences/:id" element={<NumberSequenceDetailsPage />} />

              <Route path="/audit/data-changes" element={<SimpleStatePage title="Data Changes" subtitle="Track field-level data modifications across entities." />} />
              <Route path="/audit/security-events" element={<SimpleStatePage title="Security Events" subtitle="Review high-risk events and policy violations." />} />

              <Route path="/crm-setup/lead-sources" element={<SimpleStatePage title="Lead Sources" subtitle="Manage lead source values and defaults." />} />
              <Route path="/crm-setup/industries" element={<SimpleStatePage title="Industries" subtitle="Define industry lookup values for segmentation." />} />
              <Route path="/crm-setup/case-statuses" element={<SimpleStatePage title="Case Statuses" subtitle="Configure lifecycle statuses for service cases." />} />
              <Route path="/crm-setup/opportunity-stages" element={<SimpleStatePage title="Opportunity Stages" subtitle="Manage pipeline stages and progression rules." />} />

              <Route path="/reference-data" element={<Navigate to="/admin/lookup-categories" replace />} />
            </Route>
          </Route>

          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </AuthProvider>
    </FluentProvider>
  )
}

export default App
