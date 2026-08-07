const api = '/api';
const state = { equipments: [] };
const $ = (selector, root = document) => root.querySelector(selector);
const formatNumber = value => new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 }).format(value);
const formatDate = value => new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value));

async function request(path, options = {}) {
  const response = await fetch(`${api}${path}`, { ...options, headers: { 'Content-Type': 'application/json', ...options.headers } });
  if (response.ok) return response.status === 204 ? null : response.json();
  let problem = {};
  try { problem = await response.json(); } catch { /* resposta sem corpo */ }
  throw new Error(problem.detail || problem.title || (response.status === 404 ? 'Registro não encontrado.' : 'Não foi possível concluir a operação.'));
}

function showMessage(text, success = false) {
  const message = $('#global-message');
  message.textContent = text;
  message.classList.toggle('success', success);
  message.hidden = false;
  clearTimeout(showMessage.timer);
  showMessage.timer = setTimeout(() => { message.hidden = true; }, 5000);
}

function sourceLabel(source) {
  return ({ Initial: 'Valor inicial', Manual: 'Manual', Autopilot: 'Piloto automático' })[source] || source;
}

function renderEquipments() {
  const grid = $('#equipment-grid');
  grid.replaceChildren();
  $('#equipment-empty').hidden = state.equipments.length !== 0;
  state.equipments.forEach(equipment => {
    const isNormal = equipment.currentValue >= equipment.minimumValue && equipment.currentValue <= equipment.maximumValue;
    const span = equipment.maximumValue - equipment.minimumValue;
    const progress = span <= 0 ? 0 : Math.max(0, Math.min(100, (equipment.currentValue - equipment.minimumValue) / span * 100));
    const card = document.createElement('article');
    card.className = 'equipment-card';
    card.innerHTML = `
      <div class="card-top"><div><h3></h3><div class="source"></div></div><div class="menu-actions"><button class="icon-button edit" title="Editar" aria-label="Editar equipamento">✎</button><button class="icon-button danger delete" title="Excluir" aria-label="Excluir equipamento">⌫</button></div></div>
      <div class="value-row"><div><span>Valor atual</span><strong>${formatNumber(equipment.currentValue)}</strong></div><div class="limits"><span>Faixa configurada</span><strong>${formatNumber(equipment.minimumValue)} — ${formatNumber(equipment.maximumValue)}</strong></div></div>
      <div class="range" aria-label="Posição do valor na faixa"><span class="${isNormal ? '' : 'outside'}" style="width:${progress}%"></span></div>
      <div class="value-row"><span>Atualizado em ${formatDate(equipment.updatedAt)}</span><span class="status-badge ${isNormal ? 'normal' : 'alert'}">${isNormal ? 'Dentro da faixa' : 'Fora da faixa'}</span></div>
      <div class="card-actions"><button class="button button-primary reading">＋ Registrar leitura</button><button class="button button-secondary history">Ver histórico</button></div>`;
    $('h3', card).textContent = equipment.name;
    $('.source', card).textContent = `Última origem: ${sourceLabel(equipment.lastReadingSource)}`;
    $('.edit', card).addEventListener('click', () => openEquipmentDialog(equipment));
    $('.delete', card).addEventListener('click', () => deleteEquipment(equipment));
    $('.reading', card).addEventListener('click', () => openReadingDialog(equipment));
    $('.history', card).addEventListener('click', () => openHistory(equipment));
    grid.append(card);
  });
}

async function loadDashboard() {
  $('#equipment-loading').hidden = false;
  $('#equipment-empty').hidden = true;
  try {
    const [summary, equipments] = await Promise.all([request('/dashboard/summary'), request('/equipments')]);
    state.equipments = equipments;
    $('#equipment-count').textContent = summary.equipmentCount;
    $('#reading-count').textContent = summary.readingCount;
    $('#manual-count').textContent = summary.manualReadingCount;
    $('#autopilot-count').textContent = summary.autopilotReadingCount;
    renderEquipments();
  } catch (error) {
    showMessage(`Falha ao carregar o painel: ${error.message}`);
  } finally { $('#equipment-loading').hidden = true; }
}

function openEquipmentDialog(equipment = null) {
  const form = $('#equipment-form');
  form.reset(); $('.form-error', form).hidden = true;
  $('#equipment-id').value = equipment?.id || '';
  $('#equipment-dialog-title').textContent = equipment ? 'Editar equipamento' : 'Novo equipamento';
  $('#current-value-field').hidden = Boolean(equipment);
  $('#current-value').required = !equipment;
  if (equipment) {
    $('#equipment-name').value = equipment.name;
    $('#minimum-value').value = equipment.minimumValue;
    $('#maximum-value').value = equipment.maximumValue;
  }
  $('#equipment-dialog').showModal();
}

async function submitEquipment(event) {
  event.preventDefault();
  const form = event.currentTarget, id = $('#equipment-id').value, button = $('.submit-button', form), error = $('.form-error', form);
  const payload = { name: $('#equipment-name').value.trim(), minimumValue: Number($('#minimum-value').value), maximumValue: Number($('#maximum-value').value) };
  if (!id) payload.currentValue = Number($('#current-value').value);
  button.disabled = true; error.hidden = true;
  try {
    await request(`/equipments${id ? `/${id}` : ''}`, { method: id ? 'PUT' : 'POST', body: JSON.stringify(payload) });
    $('#equipment-dialog').close(); showMessage(id ? 'Equipamento atualizado.' : 'Equipamento cadastrado.', true); await loadDashboard();
  } catch (err) { error.textContent = err.message; error.hidden = false; }
  finally { button.disabled = false; }
}

async function deleteEquipment(equipment) {
  if (!confirm(`Excluir “${equipment.name}” e todo o seu histórico?`)) return;
  try { await request(`/equipments/${equipment.id}`, { method: 'DELETE' }); showMessage('Equipamento excluído.', true); await loadDashboard(); }
  catch (error) { showMessage(error.message); }
}

function openReadingDialog(equipment) {
  const form = $('#reading-form'); form.reset(); $('.form-error', form).hidden = true;
  $('#reading-equipment-id').value = equipment.id; $('#reading-dialog-title').textContent = `Leitura · ${equipment.name}`;
  $('#reading-dialog').showModal();
}

async function submitReading(event) {
  event.preventDefault();
  const form = event.currentTarget, button = $('.submit-button', form), error = $('.form-error', form);
  button.disabled = true; error.hidden = true;
  try {
    await request(`/equipments/${$('#reading-equipment-id').value}/readings`, { method: 'POST', body: JSON.stringify({ value: Number($('#reading-value').value), source: 'Manual' }) });
    $('#reading-dialog').close(); showMessage('Leitura registrada.', true); await loadDashboard();
  } catch (err) { error.textContent = err.message; error.hidden = false; }
  finally { button.disabled = false; }
}

async function openHistory(equipment) {
  $('#history-title').textContent = `Histórico · ${equipment.name}`;
  const content = $('#history-content'); content.innerHTML = '<div class="state"><span class="spinner"></span> Carregando leituras...</div>';
  $('#history-dialog').showModal();
  try {
    const readings = await request(`/equipments/${equipment.id}/readings?limit=100`);
    if (!readings.length) { content.innerHTML = '<div class="history-empty">Nenhuma leitura encontrada.</div>'; return; }
    content.innerHTML = '<table class="history-table"><thead><tr><th>Valor</th><th>Origem</th><th>Data e hora</th></tr></thead><tbody></tbody></table>';
    const body = $('tbody', content);
    readings.forEach(reading => {
      const row = body.insertRow();
      [formatNumber(reading.value), sourceLabel(reading.source), formatDate(reading.recordedAt)].forEach(value => { const cell = row.insertCell(); cell.textContent = value; });
    });
  } catch (error) { content.innerHTML = `<div class="history-empty"></div>`; $('.history-empty', content).textContent = error.message; }
}

$('#new-equipment').addEventListener('click', () => openEquipmentDialog());
$('#refresh').addEventListener('click', loadDashboard);
$('#equipment-form').addEventListener('submit', submitEquipment);
$('#reading-form').addEventListener('submit', submitReading);
document.querySelectorAll('.close-dialog').forEach(button => button.addEventListener('click', () => $('#equipment-dialog').close()));
document.querySelectorAll('.close-reading').forEach(button => button.addEventListener('click', () => $('#reading-dialog').close()));
document.querySelectorAll('.close-history').forEach(button => button.addEventListener('click', () => $('#history-dialog').close()));
loadDashboard();
