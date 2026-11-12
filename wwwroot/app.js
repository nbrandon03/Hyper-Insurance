const state = {
  customers: [],
  vehicles: [],
  applications: [],
  policies: [],
  coverages: [],
  selectedApplicationId: null
};

const viewTitles = {
  overview: "Overview",
  customers: "Customers",
  vehicles: "Vehicles",
  applications: "Applications",
  underwriting: "Underwriting",
  policies: "Policies"
};

const byId = (id) => document.getElementById(id);
const money = (value) => new Intl.NumberFormat("en-CA", {
  style: "currency",
  currency: "CAD",
  maximumFractionDigits: 0
}).format(Number(value) || 0);

function escapeHtml(value) {
  return String(value ?? "").replace(/[&<>"']/g, (character) => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;"
  })[character]);
}

async function api(path, options = {}) {
  const response = await fetch(path, {
    ...options,
    headers: {
      Accept: "application/json",
      ...(options.body ? { "Content-Type": "application/json" } : {}),
      ...options.headers
    }
  });
  const body = await response.json().catch(() => null);
  if (!response.ok) {
    const details = body?.errors
      ? Object.values(body.errors).flat().join(" ")
      : body?.detail || body?.title || `Request failed (${response.status}).`;
    throw new Error(details);
  }
  return body;
}

function notify(message, type = "success") {
  const toast = document.createElement("div");
  toast.className = `toast${type === "error" ? " error" : ""}`;
  toast.textContent = message;
  byId("toast-region").append(toast);
  window.setTimeout(() => toast.remove(), 4800);
}

function showPanel(view) {
  if (!viewTitles[view]) return;
  document.querySelectorAll(".view-panel").forEach((panel) => {
    panel.classList.toggle("active", panel.id === `view-${view}`);
  });
  document.querySelectorAll(".nav-item").forEach((item) => {
    item.classList.toggle("active", item.dataset.view === view);
  });
  byId("current-breadcrumb").textContent = viewTitles[view];
  if (view === "underwriting") renderUnderwriting();
  if (view === "policies") renderPolicyOptions();
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function statusPill(status) {
  const known = ["approved", "active", "pending", "referral", "declined", "cancelled", "expired"];
  const className = known.includes(String(status).toLowerCase())
    ? `status-${String(status).toLowerCase()}`
    : "";
  return `<span class="status-pill ${className}">${escapeHtml(status)}</span>`;
}

function showError(containerId, error) {
  const container = byId(containerId);
  if (container) {
    container.innerHTML = `<div class="error-state">${escapeHtml(error.message || "Unable to load data.")}</div>`;
  }
}

function renderCustomers() {
  byId("customer-count").textContent = `${state.customers.length} registered`;
  byId("customer-list").innerHTML = state.customers.length
    ? state.customers.map((customer) => `
      <div class="record-row">
        <div class="record-avatar">${escapeHtml(`${customer.firstName?.[0] || ""}${customer.lastName?.[0] || ""}`.toUpperCase())}</div>
        <div class="record-main"><strong>${escapeHtml(customer.firstName)} ${escapeHtml(customer.lastName)}</strong><small>${escapeHtml(customer.email)}</small></div>
        <div class="record-side">${escapeHtml(customer.yearsLicensed)} yrs<br><span class="subcell">${escapeHtml(customer.previousAccidents)} accidents</span></div>
      </div>`).join("")
    : `<div class="empty-state">No customers yet. Register your first policyholder.</div>`;
  fillSelect(byId("application-customer"), state.customers, (item) =>
    `${item.firstName} ${item.lastName} · ${item.email}`, "Choose a customer");
}

function renderVehicles() {
  byId("vehicle-count").textContent = `${state.vehicles.length} registered`;
  byId("vehicle-list").innerHTML = state.vehicles.length
    ? state.vehicles.map((vehicle) => `
      <div class="record-row">
        <div class="record-avatar">◇</div>
        <div class="record-main"><strong>${escapeHtml(vehicle.year)} ${escapeHtml(vehicle.make)} ${escapeHtml(vehicle.model)}</strong><small>${escapeHtml(vehicle.horsepower)} hp · ${money(vehicle.value)}</small></div>
        <div class="record-side">${escapeHtml(vehicle.category)}</div>
      </div>`).join("")
    : `<div class="empty-state">No vehicles yet. Add a vehicle to start an application.</div>`;
  fillSelect(byId("application-vehicle"), state.vehicles, (item) =>
    `${item.year} ${item.make} ${item.model} · ${item.category}`, "Choose a vehicle");
}

function fillSelect(select, items, label, placeholder) {
  const previous = select.value;
  select.innerHTML = `<option value="">${escapeHtml(placeholder)}</option>${items.map((item) =>
    `<option value="${item.id}">${escapeHtml(label(item))}</option>`).join("")}`;
  if (items.some((item) => String(item.id) === previous)) select.value = previous;
}

function renderApplications() {
  const applications = [...state.applications].sort((a, b) => b.id - a.id);
  byId("application-list").innerHTML = applications.length
    ? `<table class="data-table"><thead><tr><th>Application</th><th>Policyholder / vehicle</th><th>Usage</th><th>Status</th></tr></thead><tbody>${applications.map((application) => `
      <tr><td><strong>APP-${String(application.id).padStart(5, "0")}</strong><span class="subcell">${new Date(application.submittedAt).toLocaleDateString()}</span></td>
      <td><strong>${escapeHtml(application.customer?.firstName)} ${escapeHtml(application.customer?.lastName)}</strong><span class="subcell">${escapeHtml(application.vehicle?.year)} ${escapeHtml(application.vehicle?.make)} ${escapeHtml(application.vehicle?.model)}</span></td>
      <td>${escapeHtml(application.usage)}<span class="subcell">${Number(application.annualMileage).toLocaleString()} km / yr</span></td>
      <td>${statusPill(application.status)}</td></tr>`).join("")}</tbody></table>`
    : `<div class="empty-state">No applications have been submitted.</div>`;
  const approved = state.applications.filter((application) => application.status === "Approved");
  fillSelect(byId("issue-application"), approved, (application) =>
    `APP-${String(application.id).padStart(5, "0")} · ${application.customer?.firstName || ""} ${application.customer?.lastName || ""} · ${application.vehicle?.make || ""} ${application.vehicle?.model || ""}`,
  approved.length ? "Choose an approved application" : "No approved applications yet");
}

function renderUnderwriting() {
  const applications = [...state.applications].sort((a, b) => b.id - a.id);
  byId("underwriting-list").innerHTML = applications.length
    ? `<table class="data-table"><thead><tr><th>Application</th><th>Policyholder / vehicle</th><th>Risk status</th><th>Actions</th></tr></thead><tbody>${applications.map((application) => `
      <tr><td><strong>APP-${String(application.id).padStart(5, "0")}</strong><span class="subcell">${new Date(application.submittedAt).toLocaleDateString()}</span></td>
      <td><strong>${escapeHtml(application.customer?.firstName)} ${escapeHtml(application.customer?.lastName)}</strong><span class="subcell">${escapeHtml(application.vehicle?.make)} ${escapeHtml(application.vehicle?.model)}</span></td>
      <td>${statusPill(application.status)}</td>
      <td><div class="table-actions"><button class="button button-secondary" data-action="evaluate" data-id="${application.id}">${application.status === "Pending" ? "Evaluate" : "View score"}</button>${application.status === "Referral" ? `<button class="button button-primary" data-action="review" data-id="${application.id}">Manual review</button>` : ""}</div></td></tr>`).join("")}</tbody></table>`
    : `<div class="empty-state">Submit an application to begin underwriting.</div>`;
}

function renderOverview() {
  byId("metric-customers").textContent = state.customers.length;
  byId("metric-applications").textContent = state.applications.length;
  byId("metric-pending").textContent = state.applications.filter((item) =>
    item.status === "Pending" || item.status === "Referral").length;
  byId("metric-policies").textContent = state.policies.filter((item) => item.status === "Active").length;
  byId("metric-coverages").textContent = state.coverages.length;
  const recent = [...state.applications].sort((a, b) => b.id - a.id).slice(0, 5);
  byId("recent-applications").innerHTML = recent.length
    ? `<table class="data-table"><thead><tr><th>Application</th><th>Policyholder</th><th>Vehicle</th><th>Status</th></tr></thead><tbody>${recent.map((application) => `
      <tr><td><strong>APP-${String(application.id).padStart(5, "0")}</strong></td><td>${escapeHtml(application.customer?.firstName)} ${escapeHtml(application.customer?.lastName)}</td><td>${escapeHtml(application.vehicle?.make)} ${escapeHtml(application.vehicle?.model)}</td><td>${statusPill(application.status)}</td></tr>`).join("")}</tbody></table>`
    : `<div class="empty-state">No applications yet. Start by registering a customer and vehicle.</div>`;
}

function renderPolicyOptions() {
  const container = byId("coverage-options");
  if (!state.coverages.length) {
    container.innerHTML = `<div class="empty-state">No active coverage products are available.</div>`;
    return;
  }
  container.innerHTML = state.coverages.map((coverage) => `
    <article class="coverage-option" data-coverage-card="${coverage.id}">
      <label class="coverage-identity"><input type="checkbox" class="coverage-check" value="${coverage.id}" aria-label="Select ${escapeHtml(coverage.name)}"><span><strong>${escapeHtml(coverage.name)}</strong><small>${escapeHtml(coverage.description)}</small></span></label>
      <label class="coverage-term">COVERAGE LIMIT<select data-term="limit" data-coverage="${coverage.id}" disabled>${coverage.limitOptions.map((option) => `<option value="${option.coverageLimit}" data-premium="${option.additionalPremium}">${money(option.coverageLimit)} (+${money(option.additionalPremium)})</option>`).join("")}</select></label>
      <label class="coverage-term">DEDUCTIBLE<select data-term="deductible" data-coverage="${coverage.id}" disabled>${coverage.deductibleOptions.map((option) => `<option value="${option.deductible}" data-adjustment="${option.premiumAdjustment}">${money(option.deductible)} (${Number(option.premiumAdjustment) > 0 ? "+" : ""}${money(option.premiumAdjustment)})</option>`).join("")}</select></label>
    </article>`).join("");
  refreshEstimatedPremium();
}

function renderPolicies() {
  const policies = [...state.policies].sort((a, b) => b.id - a.id);
  byId("policy-list").innerHTML = policies.length
    ? `<table class="data-table"><thead><tr><th>Policy</th><th>Policyholder</th><th>Annual premium</th><th>Coverage</th><th>Status</th><th>Manage</th></tr></thead><tbody>${policies.map((policy) => {
      const application = state.applications.find((item) => item.id === policy.applicationId);
      return `<tr><td><button class="text-button" data-action="details" data-id="${policy.id}">${escapeHtml(policy.policyNumber)}</button><span class="subcell">APP-${String(policy.applicationId).padStart(5, "0")}</span></td><td>${escapeHtml(application ? `${application.customer?.firstName || ""} ${application.customer?.lastName || ""}` : "—")}</td><td><strong>${money(policy.annualPremium)}</strong></td><td>${policy.coverages?.length || 0} selected</td><td>${statusPill(policy.status)}</td><td><div class="table-actions">${policy.status === "Active" ? `<button class="button button-secondary" data-action="details" data-id="${policy.id}">Details</button><button class="button button-danger" data-action="cancel" data-id="${policy.id}">Cancel</button><button class="button button-secondary" data-action="renew" data-id="${policy.id}">Renew</button>` : `<button class="button button-secondary" data-action="details" data-id="${policy.id}">Details</button>`}</div></td></tr>`;
    }).join("")}</tbody></table>`
    : `<div class="empty-state">No policies issued yet. Approve an application to bind coverage.</div>`;
}

function renderAll() {
  renderCustomers();
  renderVehicles();
  renderApplications();
  renderUnderwriting();
  renderPolicyOptions();
  renderPolicies();
  renderOverview();
}

async function loadResource(key, path, render, containerId) {
  try {
    state[key] = await api(path);
    render();
  } catch (error) {
    state[key] = [];
    showError(containerId, error);
    notify(error.message, "error");
  }
}

async function refreshCustomers() {
  await loadResource("customers", "/api/customers", () => { renderCustomers(); renderOverview(); }, "customer-list");
}

async function refreshVehicles() {
  await loadResource("vehicles", "/api/vehicles", () => { renderVehicles(); }, "vehicle-list");
}

async function refreshApplications() {
  await loadResource("applications", "/api/applications", () => {
    renderApplications();
    renderUnderwriting();
    renderOverview();
  }, "application-list");
}

async function refreshPolicies() {
  await loadResource("policies", "/api/policies", () => { renderPolicies(); renderOverview(); }, "policy-list");
}

async function refreshCoverages() {
  await loadResource("coverages", "/api/coverages", () => { renderPolicyOptions(); renderOverview(); }, "coverage-options");
}

async function refreshAll() {
  await Promise.all([
    refreshCustomers(),
    refreshVehicles(),
    refreshApplications(),
    refreshPolicies(),
    refreshCoverages()
  ]);
}

function formData(form) {
  return Object.fromEntries(new FormData(form).entries());
}

function numericFields(body, fields) {
  for (const field of fields) body[field] = Number(body[field]);
  return body;
}

async function submitForm(form, path, success, transform = (body) => body, refresh = refreshAll) {
  const button = form.querySelector('button[type="submit"]');
  const original = button.textContent;
  button.disabled = true;
  button.textContent = "Saving…";
  try {
    await api(path, { method: "POST", body: JSON.stringify(transform(formData(form))) });
    form.reset();
    notify(success);
    await refresh();
  } catch (error) {
    notify(error.message, "error");
  } finally {
    button.disabled = false;
    button.textContent = original;
  }
}

function refreshEstimatedPremium() {
  const applicationId = Number(byId("issue-application").value);
  const application = state.applications.find((item) => item.id === applicationId);
  const quote = byId("quote-preview").querySelector("strong");
  if (!application) {
    quote.textContent = "Select an application";
    return;
  }
  let base = Number(application._basePremium) || 0;
  if (!application._basePremium) {
    api(`/api/rating/quote/${applicationId}`).then((result) => {
      application._basePremium = result.annualPremium;
      refreshEstimatedPremium();
    }).catch(() => { quote.textContent = "Quote unavailable"; });
  }
  const selectedPremium = [...document.querySelectorAll(".coverage-check:checked")]
    .reduce((sum, checkbox) => {
      const card = checkbox.closest(".coverage-option");
      const limit = card.querySelector('[data-term="limit"]');
      const deductible = card.querySelector('[data-term="deductible"]');
      const limitAdjustment = Number(limit.selectedOptions[0]?.dataset.premium) || 0;
      const deductibleAdjustment = Number(deductible.selectedOptions[0]?.dataset.adjustment) || 0;
      return sum + limitAdjustment + deductibleAdjustment;
    }, 0);
  quote.textContent = base ? money(base + selectedPremium) : "Calculating…";
}

function renderRiskResult(result) {
  byId("underwriting-result").classList.remove("hidden");
  byId("result-title").textContent = `Application APP-${String(result.applicationId).padStart(5, "0")}`;
  const factors = result.riskFactors?.length
    ? result.riskFactors.map((factor) => `<div class="factor-row"><strong>${escapeHtml(factor.rule)}</strong><span>${escapeHtml(factor.explanation)}</span><b>+${Number(factor.points).toLocaleString()} pts</b></div>`).join("")
    : `<div class="empty-state">No additional risk factors were recorded.</div>`;
  byId("result-content").innerHTML = `
    <div class="risk-summary"><div class="risk-score"><strong>${Number(result.riskScore).toLocaleString()}</strong><small>RISK SCORE</small></div><div><h3>${statusPill(result.decision)}</h3><p>${escapeHtml(result.reason)}</p></div></div>
    <div class="factor-list">${factors}</div>
    ${result.reviewedBy ? `<div class="review-note"><strong>Reviewed by ${escapeHtml(result.reviewedBy)}</strong><p>${escapeHtml(result.reviewNotes)}</p></div>` : ""}`;
  byId("underwriting-result").scrollIntoView({ behavior: "smooth", block: "nearest" });
}

async function openEvaluation(applicationId) {
  try {
    const result = await api(`/api/underwriting/application/${applicationId}`);
    renderRiskResult(result);
  } catch (error) {
    notify(error.message, "error");
  }
}

async function evaluateApplication(applicationId) {
  try {
    const result = await api(`/api/underwriting/evaluate/${applicationId}`, { method: "POST" });
    const application = state.applications.find((item) => item.id === applicationId);
    if (application) application.status = result.decision;
    renderUnderwriting();
    renderApplications();
    renderOverview();
    renderPolicyOptions();
    renderRiskResult(result);
    notify(`Evaluation complete · ${result.decision}`);
  } catch (error) {
    notify(error.message, "error");
  }
}

async function openManualReview(applicationId) {
  try {
    const result = await api(`/api/underwriting/application/${applicationId}`);
    renderRiskResult(result);
    const application = state.applications.find((item) => item.id === applicationId);
    byId("review-application-label").textContent = `APP-${String(applicationId).padStart(5, "0")} · ${application?.customer?.firstName || ""} ${application?.customer?.lastName || ""} · Current score ${result.riskScore}`;
    byId("review-form").elements.applicationId.value = applicationId;
    byId("manual-review").classList.remove("hidden");
    byId("manual-review").scrollIntoView({ behavior: "smooth", block: "center" });
  } catch (error) {
    notify(error.message, "error");
  }
}

function openPolicyModal(policy) {
  const coverageRows = policy.coverages?.length
    ? `<div class="table-wrap"><table class="data-table"><thead><tr><th>Coverage</th><th>Limit</th><th>Deductible</th><th>Premium impact</th></tr></thead><tbody>${policy.coverages.map((coverage) => `<tr><td><strong>${escapeHtml(coverage.name)}</strong><span class="subcell">${escapeHtml(coverage.description)}</span></td><td>${money(coverage.coverageLimit)}</td><td>${money(coverage.deductible)}</td><td>${money(coverage.additionalPremium)}</td></tr>`).join("")}</tbody></table></div>`
    : `<div class="empty-state">No coverage details saved.</div>`;
  byId("modal-title").textContent = policy.policyNumber;
  byId("modal-content").innerHTML = `
    <div class="policy-detail-grid">
      <div class="detail-tile"><span>Annual premium</span><strong>${money(policy.annualPremium)}</strong></div>
      <div class="detail-tile"><span>Status</span><strong>${escapeHtml(policy.status)}</strong></div>
      <div class="detail-tile"><span>Effective</span><strong>${new Date(policy.startDate).toLocaleDateString()}</strong></div>
      <div class="detail-tile"><span>Expires</span><strong>${new Date(policy.expiryDate).toLocaleDateString()}</strong></div>
      <div class="detail-tile"><span>Application</span><strong>APP-${String(policy.applicationId).padStart(5, "0")}</strong></div>
      <div class="detail-tile"><span>Selected coverages</span><strong>${policy.coverages?.length || 0}</strong></div>
    </div>${coverageRows}`;
  byId("policy-modal").classList.remove("hidden");
}

document.addEventListener("click", async (event) => {
  const nav = event.target.closest("[data-view]");
  if (nav) showPanel(nav.dataset.view);
  const go = event.target.closest("[data-go]");
  if (go) showPanel(go.dataset.go);

  const refresh = event.target.closest("[data-refresh]");
  if (refresh) {
    const refreshers = {
      customers: refreshCustomers,
      vehicles: refreshVehicles,
      applications: refreshApplications,
      underwriting: refreshApplications,
      policies: refreshPolicies,
      coverages: refreshCoverages
    };
    await refreshers[refresh.dataset.refresh]?.();
  }
  if (event.target.closest("#refresh-button")) await refreshAll();

  const action = event.target.closest("[data-action]");
  if (action) {
    const id = Number(action.dataset.id);
    if (action.dataset.action === "evaluate") {
      const application = state.applications.find((item) => item.id === id);
      if (application?.status === "Pending") await evaluateApplication(id);
      else await openEvaluation(id);
    }
    if (action.dataset.action === "review") await openManualReview(id);
    if (action.dataset.action === "details") {
      const policy = await api(`/api/policies/${id}`).catch((error) => {
        notify(error.message, "error");
        return null;
      });
      if (policy) openPolicyModal(policy);
    }
    if (action.dataset.action === "cancel") {
      if (!window.confirm("Cancel this active policy?")) return;
      try {
        await api(`/api/policies/${id}/cancel`, { method: "POST" });
        notify("Policy cancelled.");
        await refreshAll();
      } catch (error) { notify(error.message, "error"); }
    }
    if (action.dataset.action === "renew") {
      try {
        await api(`/api/policies/${id}/renew`, { method: "POST" });
        notify("Policy renewed for one year.");
        await refreshAll();
      } catch (error) { notify(error.message, "error"); }
    }
  }
  if (event.target.closest("#close-result")) byId("underwriting-result").classList.add("hidden");
  if (event.target.closest("#close-modal") || event.target.id === "policy-modal") {
    byId("policy-modal").classList.add("hidden");
  }
});

document.addEventListener("change", (event) => {
  if (event.target.matches(".coverage-check")) {
    const card = event.target.closest(".coverage-option");
    card.classList.toggle("is-selected", event.target.checked);
    card.querySelectorAll(".coverage-term select").forEach((select) => {
      select.disabled = !event.target.checked;
    });
    refreshEstimatedPremium();
  }
  if (event.target.matches(".coverage-term select")) refreshEstimatedPremium();
  if (event.target.id === "issue-application") refreshEstimatedPremium();
});

byId("customer-form").addEventListener("submit", (event) => {
  event.preventDefault();
  submitForm(event.currentTarget, "/api/customers", "Customer registered.", (body) =>
    numericFields(body, ["yearsLicensed", "previousAccidents", "trafficViolations"]), refreshCustomers);
});

byId("vehicle-form").addEventListener("submit", (event) => {
  event.preventDefault();
  submitForm(event.currentTarget, "/api/vehicles/classify", "Vehicle classified and registered.", (body) =>
    numericFields(body, ["year", "value", "horsepower"]), refreshVehicles);
});

byId("application-form").addEventListener("submit", (event) => {
  event.preventDefault();
  submitForm(event.currentTarget, "/api/applications", "Application submitted.", (body) =>
    numericFields(body, ["customerId", "vehicleId", "annualMileage"]), refreshApplications);
});

byId("review-form").addEventListener("submit", async (event) => {
  event.preventDefault();
  const form = event.currentTarget;
  const body = formData(form);
  const applicationId = body.applicationId;
  delete body.applicationId;
  try {
    const result = await api(`/api/underwriting/review/${applicationId}`, {
      method: "POST",
      body: JSON.stringify(body)
    });
    const application = state.applications.find((item) => item.id === Number(applicationId));
    if (application) application.status = result.decision;
    byId("manual-review").classList.add("hidden");
    renderRiskResult(result);
    renderAll();
    notify(`Manual review saved · ${result.decision}`);
  } catch (error) { notify(error.message, "error"); }
});

byId("issue-form").addEventListener("submit", async (event) => {
  event.preventDefault();
  const applicationId = Number(byId("issue-application").value);
  if (!applicationId) {
    notify("Choose an approved application first.", "error");
    return;
  }
  const coverages = [...document.querySelectorAll(".coverage-check:checked")].map((checkbox) => {
    const card = checkbox.closest(".coverage-option");
    return {
      coverageId: Number(checkbox.value),
      coverageLimit: Number(card.querySelector('[data-term="limit"]').value),
      deductible: Number(card.querySelector('[data-term="deductible"]').value)
    };
  });
  const button = event.currentTarget.querySelector('button[type="submit"]');
  button.disabled = true;
  try {
    const policy = await api(`/api/policies/issue/${applicationId}`, {
      method: "POST",
      body: JSON.stringify({ coverages })
    });
    notify(`Policy ${policy.policyNumber} issued successfully.`);
    event.currentTarget.reset();
    renderPolicyOptions();
    await refreshAll();
    openPolicyModal(policy);
  } catch (error) { notify(error.message, "error"); }
  finally { button.disabled = false; }
});

byId("close-modal").addEventListener("click", () => byId("policy-modal").classList.add("hidden"));
document.addEventListener("keydown", (event) => {
  if (event.key === "Escape") byId("policy-modal").classList.add("hidden");
});

refreshAll();
