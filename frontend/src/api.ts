export async function api<T>(path: string): Promise<T> {
  const response = await fetch(`/api${path}`);

  if (!response.ok) {
    throw new Error('Não foi possível carregar os dados.');
  }

  return response.json() as Promise<T>;
}
