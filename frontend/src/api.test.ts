import { afterEach, describe, expect, it, vi } from 'vitest';
import { api } from './api';

describe('api', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('returns the JSON response from the versioned API path', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ equipmentCount: 3 }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await expect(api<{ equipmentCount: number }>('/dashboard/summary')).resolves.toEqual({
      equipmentCount: 3,
    });
    expect(fetchMock).toHaveBeenCalledWith('/api/dashboard/summary');
  });

  it('surfaces a consistent error when the API request fails', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false }));

    await expect(api('/dashboard/summary')).rejects.toThrow(
      'Não foi possível carregar os dados.',
    );
  });
});
